// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Importing.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    private async Task<ResourceHandle> ImportSingleResource(ResourceID resourceId) {
        if (await GetOrCreateResourceContainer(resourceId, typeof(object)) is not { } container) {
            return new(resourceId, null);
        }
        
        Debug.Assert(container.ReferenceWaitTask != null);
        
        return new(resourceId, await container.ReferenceWaitTask);
    }

    private async Task<ResourceHandle<T>> ImportSingleResource<T>(ResourceID resourceId) where T : class {
        if (await GetOrCreateResourceContainer(resourceId, typeof(object)) is not { } container) {
            return new(resourceId, null);
        }
        
        Debug.Assert(container.ReferenceWaitTask != null);
        
        object resource = await container.ReferenceWaitTask;
        return new(resourceId, resource as T);
    }

    private async Task<ResourceContainer?> GetOrCreateResourceContainer(ResourceID resourceId, Type resourceType) {
        if (resourceId == ResourceID.Null) return null;
        
        if (!_environment.Libraries.ContainsResource(resourceId)) {
            Log.UnregisteredResource(_environment.Logger, resourceId);
            return null;
        }
        
        await _containerLock.WaitAsync();
        try {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, resourceId, out bool exists);
            
            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
        
                container.ReferenceCount++;
            } else {
                Log.BeginImport(_environment.Logger, resourceId);
        
                container = new(resourceId);
                BeginImport(container, resourceType);
            }
            
            _environment.Statistics.AddReference();
            return container;
        } finally {
            _containerLock.Release();
        }
    }
    
    void BeginImport(ResourceContainer container, Type resourceType) {
        container.VesselImportTask = ImportResource(container, resourceType);
        container.ReferenceResolveTask = ResolveReference(container);
        container.ReferenceWaitTask = WaitReferencesFinish(container);
        
        return;

        async Task<VesselDeserializeResult> ImportResource(ResourceContainer container, Type resourceType) {
            await Task.Yield();

            if (_environment.Libraries.CreateResourceStream(container.ResourceId) is not { } resourceStream) {
                return await Task.FromException<VesselDeserializeResult>(new InvalidOperationException($"Null resource stream provided despite contains resource '{container.ResourceId}'."));
            }

            var layout = BinaryHeader.Extract(resourceStream);

            switch (layout.MajorVersion) {
                case 1: return await ImportResourceVesselV1(resourceType, resourceStream, layout, container.CancellationTokenSource.Token);

                default:
                    await resourceStream.DisposeAsync();
                    throw new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported.");
            }
        }

        async Task<VesselDeserializeResult> ImportResourceVesselV1(Type type, Stream resourceStream, BinaryHeader header, CancellationToken cancellationToken) {
            bool disposeResourceStream = true;

            try {
                if (header.MinorVersion != 0) {
                    throw new NotSupportedException($"Compiled resource version 1.{header.MinorVersion} is not supported.");
                }

                if (!header.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out ChunkInformation dataChunkInfo)) {
                    throw new CorruptedBinaryException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
                }

                if (!header.TryGetChunkInformation(CompilingConstants.DeserializationChunkTag, out ChunkInformation deserializationChunkInfo)) {
                    throw new CorruptedBinaryException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.DeserializationChunkTag)} chunk.");
                }

                resourceStream.Seek(deserializationChunkInfo.ContentOffset, SeekOrigin.Begin);

                using BinaryReader reader = new(resourceStream, Encoding.UTF8, true);

                string deserializerName = reader.ReadString();

                if (!_environment.Deserializers.TryGetValue(deserializerName, out Deserializer? deserializer)) {
                    throw new ArgumentException($"Deserializer name '{deserializerName}' is unregistered.");
                }

                if (!deserializer.OutputType.IsAssignableTo(type)) {
                    return default;
                }

                await using Stream optionsStream = await CopyOptionsStreamAsync(resourceStream, header, cancellationToken);

                resourceStream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
                optionsStream.Seek(0, SeekOrigin.Begin);

                await using PartialReadStream dataStream = new(resourceStream, dataChunkInfo.ContentOffset, dataChunkInfo.Length, ownStream: false);
                DeserializationContext context = new(_environment.Logger);

                object deserialized = await deserializer.DeserializeObjectAsync(dataStream, optionsStream, context, cancellationToken);

                if (deserializer.Streaming) {
                    disposeResourceStream = false;
                }

                return new(deserializer, deserialized, context);
            } finally {
                if (disposeResourceStream) await resourceStream.DisposeAsync();
            }
            
            static async Task<Stream> CopyOptionsStreamAsync(Stream stream, BinaryHeader header, CancellationToken token) {
                if (header.TryGetChunkInformation(CompilingConstants.ImportOptionsChunkTag, out ChunkInformation optionsChunkInfo)) {
                    stream.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
                    
                    MemoryStream optionsStream = new((int)optionsChunkInfo.Length);
                    await stream.CopyToAsync(optionsStream, (int)optionsChunkInfo.Length, token, 128);
            
                    return optionsStream;
                }
                
                return Stream.Null;
            }
        }

        async Task<object> ResolveReference(ResourceContainer container) {
            try {
                Debug.Assert(container.VesselImportTask != null);

                (Deserializer deserializer, object deserialized, DeserializationContext context) = await container.VesselImportTask;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (deserialized == null) {
                    throw new InvalidOperationException("Deserializer cannot returns null object.");
                }

                bool add = _importedObjectMap.TryAdd(deserialized, container);
                Debug.Assert(add);
                
                _environment.Statistics.IncrementUniqueResourceCount();

                // Import the references.
                
                // Get the ResourceContainers of the reference resources.
                // TODO: Make a specialize function that lock and grab all at once instead of locking each one.
                Dictionary<ReferencePropertyKey, Task<ResourceContainer?>> referenceContainerTasks = context.RequestingReferences.ToDictionary(kvp => kvp.Key, kvp => GetOrCreateResourceContainer(kvp.Value.ResourceId, kvp.Value.Type));
                await Task.WhenAll(referenceContainerTasks.Values);

                // Wait for reference resources to finish importing the vessel.
                Dictionary<ReferencePropertyKey, ResourceContainer> referenceContainers = referenceContainerTasks.Where(x => x.Value is { IsCompletedSuccessfully: true, Result: not null }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result)!;
                
                await Task.WhenAll(referenceContainers.Values.Select(async referenceContainer => {
                    try {
                        Debug.Assert(referenceContainer.VesselImportTask != null);
                        return await referenceContainer.VesselImportTask;
                    } catch (Exception e) {
                        // _environment.Logger.LogError(Log.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                        return default;
                    }
                }));

                // Resolve references.
                try {
                    context.References = referenceContainers.Where(x => x.Value.VesselImportTask!.IsCompletedSuccessfully).Select(x => KeyValuePair.Create(x.Key, x.Value.VesselImportTask.Result.Output)).Where(x => x.Value != null!).ToDictionary()!;
                    
                    deserializer.ResolveReferences(deserialized, context);
                } catch (Exception e) {
                    // _environment.Logger.LogError(Logging.ResolveDependenciesEvent, e, "Exception occured while resolving dependencies.");
                }

                container.ReferenceContainers = referenceContainers.Count == 0 ? FrozenSet<ResourceContainer>.Empty : referenceContainers.Select(x => x.Value).ToFrozenSet(ReferenceResourceContainerComparer.Instance);

                return deserialized;
            } catch {
                await _containerLock.WaitAsync();

                try {
                    _resourceContainers.Remove(container.ResourceId);
                    _environment.Statistics.Release(container.ReferenceCount);
                } finally {
                    _containerLock.Release();
                }

                throw;
            } finally {
                container.CancellationTokenSource.Dispose();
            }
        }

        async Task<object> WaitReferencesFinish(ResourceContainer container) {
            object? result = await container.ReferenceResolveTask!;
            
            Debug.Assert(result != null, "Unexpected null result.");
            
            await Task.WhenAll(container.ReferenceContainers.Select(x => x.ReferenceResolveTask!));

            return result;
        }
    }
    
    internal readonly record struct VesselDeserializeResult(Deserializer Deserializer, object Output, DeserializationContext Context);

    internal sealed class ReferenceResourceContainerComparer : IEqualityComparer<ResourceContainer> {
        public static ReferenceResourceContainerComparer Instance { get; } = new();
        
        public bool Equals(ResourceContainer? x, ResourceContainer? y) {
            Debug.Assert(x is not null && y is not null);

            return x.ResourceId == y.ResourceId;
        }
        
        public int GetHashCode(ResourceContainer obj) {
            return obj.ResourceId.GetHashCode();
        }
    }
}
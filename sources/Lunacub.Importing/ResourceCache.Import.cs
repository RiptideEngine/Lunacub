using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Importing.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    private async Task<ResourceHandle> ImportSingleResource(ResourceID rid) {
        // Fast and dirty way.
        return new(rid, await ImportSingleResource<object>(rid));
    }
    
    private async Task<ResourceHandle<T>> ImportSingleResource<T>(ResourceID rid) where T : class {
        if (rid == ResourceID.Null) return new(rid, null);
        
        if (!_environment.Libraries.ContainResource(rid)) {
            Logging.UnregisteredResource(_environment.Logger, rid);
            return new(rid, null);
        }

        Task<object?> importingTask;
        
        await _containerLock.WaitAsync();
        try {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                
                container.ReferenceCount++;
                importingTask = container.FullImportTask;
            } else {
                Logging.BeginImport(_environment.Logger, rid);
                
                container = new(rid);
                container.VesselImportTask = ImportResourceVessel(rid, typeof(T), container);

                importingTask = container.FullImportTask = FullImport(rid, container, [rid]);
            }
            
            _environment.Statistics.AddReference();
        } finally {
            _containerLock.Release();
        }
        
        return new(rid, await importingTask as T);
    }
    
    private ResourceContainer? ImportReferenceResource(ResourceID rid, HashSet<ResourceID> stack) {
        if (rid == ResourceID.Null) return null;
        
        if (!_environment.Libraries.ContainResource(rid)) {
            Logging.UnregisteredDependencyResource(_environment.Logger, rid);
            return null;
        }

        lock (stack) {
            stack.Add(rid);
        }

        _containerLock.Wait();
        try {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                
                container.ReferenceCount++;
            } else {
                Logging.BeginImportReference(_environment.Logger, rid);
                
                container = new(rid);
                container.VesselImportTask = ImportResourceVessel(rid, typeof(object), container);
                container.FullImportTask = FullImport(rid, container, stack);
            }

            _environment.Statistics.AddReference();
            return container;
        } finally {
            _containerLock.Release();
        }
    }
    
    private async Task<DeserializeResult> ImportResourceVessel(ResourceID rid, Type type, ResourceContainer container) {
        await Task.Yield();
        
        if (_environment.Libraries.CreateResourceStream(rid) is not { } resourceStream) {
            return await Task.FromException<DeserializeResult>(new InvalidOperationException($"Null resource stream provided despite contains resource '{rid}'."));
        }
        
        var layout = LayoutExtracting.Extract(resourceStream);

        switch (layout.MajorVersion) {
            case 1: return await ImportResourceVesselV1(type, resourceStream, layout, container.CancellationTokenSource.Token);
                
            default:
                await resourceStream.DisposeAsync();
                throw new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported.");
        }
    }

    private async Task<object?> FullImport(ResourceID rid, ResourceContainer container, HashSet<ResourceID> stack) {
        try {
            (Deserializer deserializer, object deserialized, DeserializationContext context) = await container.VesselImportTask;

            // TODO: Handle invalid null return.
            if (deserialized == null!) return null;
            
            _environment.Statistics.IncrementUniqueResourceCount();
            
            // Import dependencies references.
            Dictionary<ReferencePropertyKey, ResourceContainer> importedDependencies = [];
            foreach ((ReferencePropertyKey property, DeserializationContext.RequestingDependency requesting) in context.RequestingDependencies) {
                if (ImportReferenceResource(requesting.Rid, stack) is not { } dependencyContainer) continue;
                
                importedDependencies.Add(property, dependencyContainer);
            }
            
            // Wait for all dependencies to finish import it's vessel and then resolve dependencies.
            await Task.WhenAll(importedDependencies.Values.Select(async dependencyContainer => {
                try {
                    Debug.Assert(dependencyContainer.VesselImportTask != null);
                    return await dependencyContainer.VesselImportTask;
                } catch (Exception e) {
                    // _environment.Logger.LogError(Logging.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                    return default;
                }
            }));
                    
            context.References = importedDependencies.Where(x => x.Value.VesselImportTask!.IsCompletedSuccessfully).Select(x => KeyValuePair.Create(x.Key, x.Value.VesselImportTask.Result.Output)).Where(x => x.Value != null!).ToDictionary()!;
            
            try {
                deserializer.ResolveReferences(deserialized, context);
            } catch (Exception e) {
                // _environment.Logger.LogError(Logging.ResolveDependenciesEvent, e, "Exception occured while resolving dependencies.");
            }
            
            // Wait for the dependencies to fully finishing import (filter the stack to prevent deadlock).
            IEnumerable<ResourceContainer> filteredContainers;
            
            lock (stack) {
                filteredContainers = importedDependencies.Values.ExceptBy(stack, c => c.ResourceId);
            }
            
            await Task.WhenAll(filteredContainers.Select(async x => {
                try {
                    return await x.FullImportTask;
                } catch (Exception e) {
                    // _environment.Logger.LogError(Logging.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                    return null;
                }
            }));
            
            lock (_importedReleaseCache) {
                _importedReleaseCache.Add(deserialized, container);
            }
            
            return deserialized;
        } catch {
            await _containerLock.WaitAsync();
            try {
                _resourceContainers.Remove(rid);
                _environment.Statistics.Release(container.ReferenceCount);
                container.ReferenceCount = 0;
            } finally {
                _containerLock.Release();
            }

            throw;
        } finally {
            Debug.Assert(container.VesselImportTask.Status != TaskStatus.Running);
            container.CancellationTokenSource.Dispose();
        }
    }
    
    private async Task<DeserializeResult> ImportResourceVesselV1(Type type, Stream resourceStream, CompiledResourceLayout layout, CancellationToken cancellationToken) {
        bool disposeResourceStream = true;
        
        try {
            if (layout.MinorVersion != 0) {
                throw new NotSupportedException($"Compiled resource version 1.{layout.MinorVersion} is not supported.");
            }
        
            if (!layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out ChunkInformation dataChunkInfo)) {
                throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
            }
        
            if (!layout.TryGetChunkInformation(CompilingConstants.DeserializationChunkTag, out ChunkInformation deserializationChunkInfo)) {
                throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.DeserializationChunkTag)} chunk.");
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
            
            await using Stream optionsStream = await CopyOptionsStreamAsync(resourceStream, layout, cancellationToken);

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
    
        static async Task<Stream> CopyOptionsStreamAsync(Stream stream, CompiledResourceLayout layout, CancellationToken token) {
            if (layout.TryGetChunkInformation(CompilingConstants.ImportOptionsChunkTag, out ChunkInformation optionsChunkInfo)) {
                stream.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
                
                MemoryStream optionsStream = new((int)optionsChunkInfo.Length);
                await stream.CopyToAsync(optionsStream, (int)optionsChunkInfo.Length, token, 128);
    
                return optionsStream;
            }
            
            return Stream.Null;
        }
    }
}
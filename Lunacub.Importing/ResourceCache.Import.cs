using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    private Task<object?> ImportSingleResource(ResourceID rid) {
        if (!_environment.Input.ContainResource(rid)) return Task.FromResult<object?>(null);

        using (_cacheLock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_cacheDict, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                container.ReferenceCount++;

                return container.Task;
            }
            
            Dictionary<ResourceID, Task<object?>> importStack = [];
            CancellationTokenSource cts = new();
            
            container = new(BeginImport(rid, importStack, cts), 1, cts);

            return container.Task;
        }
    }

    private Task<object?> GetOrBeginImportAsync(ResourceID rid, Dictionary<ResourceID, Task<object?>> importStack) {
        if (!_environment.Input.ContainResource(rid)) return Task.FromResult<object?>(null);
        
        using (_cacheLock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_cacheDict, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                container.ReferenceCount++;

                return container.Task;
            }
            
            CancellationTokenSource cts = new();
            container = new(BeginImport(rid, importStack, cts), 1, cts);

            return container.Task;
        }
    }

    private async Task<object?> BeginImport(ResourceID rid, Dictionary<ResourceID, Task<object?>> importStack, CancellationTokenSource cts) {
        Task<object?> task;
        
        lock (importStack) {
            if (importStack.TryGetValue(rid, out var importingTask)) {
                using (_cacheLock.EnterScope()) {
                    _cacheDict[rid].ReferenceCount++;
                }

                return importingTask;
            }
            
            _environment.Logger.LogInformation(Logging.BeginImportEvent, "Begin import resource {rid}.", rid);
            
            task = ImportResourceInner(rid, importStack, cts.Token);
            importStack.Add(rid, task);
        }
        
        try {
            return await task;
        } catch (OperationCanceledException) {
            _environment.Logger.LogInformation(Logging.ImportCancelEvent, "Cancel import resource {rid}.", rid);
            return Task.FromCanceled<object?>(CancellationToken.None);
        } catch (Exception e) {
            cts.Dispose();
            _environment.Logger.LogError(Logging.ImportExceptionOccuredEvent, e, "Failed to import resource {rid}.", rid);;
            
            _cacheDict.Remove(rid);
            return Task.FromException<object?>(e);
        }
    }

    private async Task<object?> ImportResourceInner(ResourceID rid, Dictionary<ResourceID, Task<object?>> importStack, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        
        Stream? resourceStream = _environment.Input.CreateResourceStream(rid);
        
        if (resourceStream == null) {
            throw new InvalidOperationException($"Null resource stream provided despite contains resource '{rid}'.");
        }
        
        var layout = LayoutExtracting.Extract(resourceStream);

        switch (layout.MajorVersion) {
            case 1: return await ImportV1(rid, resourceStream, layout, importStack, cancellationToken);
            default:
                await resourceStream.DisposeAsync();
                throw new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported.");
        }
    }

    private async Task<object?> ImportV1(ResourceID rid, Stream resourceStream, CompiledResourceLayout layout, Dictionary<ResourceID, Task<object?>> importStack, CancellationToken cancellationToken) {
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
            
            DeserializationContext context;
            object deserialized;
            
            await using (Stream optionsStream = CopyOptionsStream(resourceStream, in layout)) {
                resourceStream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
                optionsStream.Seek(0, SeekOrigin.Begin);
            
                await using PartialReadStream dataStream = new(resourceStream, dataChunkInfo.ContentOffset, dataChunkInfo.Length, ownStream: false);
                context = new();
                
                deserialized = deserializer.DeserializeObject(dataStream, optionsStream, context);
            
                if (deserializer.Streaming) {
                    disposeResourceStream = false;
                }
            }
            
            Dictionary<string, Task<object?>> importedDependencies = [];
            
            foreach ((string property, DeserializationContext.RequestingDependency requesting) in context.RequestingDependencies) {
                importedDependencies.Add(property, GetOrBeginImportAsync(requesting.Rid, importStack));
            }
            
            await Task.WhenAll(importedDependencies.Values.Select(async x => {
                try {
                    return await x;
                } catch (Exception e) {
                    _environment.Logger.LogError(Logging.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                    return null;
                }
            }));
            
            context.Dependencies = importedDependencies.Where(x => x.Value.IsCompletedSuccessfully).Select(x => KeyValuePair.Create(x.Key, x.Value.Result)).ToDictionary();
            deserializer.ResolveDependencies(deserialized, context);

            return deserialized;
        } finally {
            if (disposeResourceStream) await resourceStream.DisposeAsync();
        }

        static Stream CopyOptionsStream(Stream stream, in CompiledResourceLayout layout) {
            if (layout.TryGetChunkInformation(CompilingConstants.ImportOptionsChunkTag, out ChunkInformation optionsChunkInfo)) {
                stream.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
                
                MemoryStream optionsStream = new((int)optionsChunkInfo.Length);
                stream.CopyTo(optionsStream, (int)optionsChunkInfo.Length, 512);

                return optionsStream;
            }

            return Stream.Null;
        }
    }

    private Task<object?> ImportSingleResourceRecursive(ResourceID rid, IDictionary<ResourceID, Task<object?>> importStack) {
        throw new NotImplementedException();

        // ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_cacheDict, rid);
        //
        // if (!Unsafe.IsNullRef(ref container)) {
        //     Debug.Assert(container.ReferenceCount != 0);
        //
        //     container.ReferenceCount++;
        //     return container.Value;
        // }
    }

    private Task<object?> ImportSingleResource(ResourceID rid, Type type) {
        if (_environment.Input.ContainResource(rid)) return Task.FromResult<object?>(null);

        throw new NotImplementedException();
    }
}
using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    private Task<object?> ImportSingleResource(ResourceID rid) {
        if (!_context.Input.ContainResource(rid)) return Task.FromResult<object?>(null);

        using (_cacheLock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_cacheDict, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                container.ReferenceCount++;

                return container.Task;
            }
            
            ConcurrentDictionary<ResourceID, Task<object?>> importStack = [];
            container = new(BeginImport(rid, importStack));

            return container.Task;
        }
    }

    private Task<object?> GetOrBeginImportAsync(ResourceID rid, IDictionary<ResourceID, Task<object?>> importStack) {
        if (!_context.Input.ContainResource(rid)) return Task.FromResult<object?>(null);
        
        using (_cacheLock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_cacheDict, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                container.ReferenceCount++;

                return container.Task;
            }
            
            container = new(BeginImport(rid, importStack));

            return container.Task;
        }
    }

    private Task<object?> BeginImport(ResourceID rid, IDictionary<ResourceID, Task<object?>> importStack) {
        if (importStack.TryGetValue(rid, out var importingTask)) {
            using (_cacheLock.EnterScope()) {
                _cacheDict[rid].ReferenceCount++;
            }

            return importingTask;
        }

        try {
            Task<object?> task = ImportResourceInner(rid, importStack);
            importStack.Add(rid, task);

            return task;
        } catch (Exception e) {
            _cacheDict.Remove(rid);
            return Task.FromException<object?>(e);
        }
    }

    private async Task<object?> ImportResourceInner(ResourceID rid, IDictionary<ResourceID, Task<object?>> importStack) {
        Stream? resourceStream = _context.Input.CreateResourceStream(rid);
        
        if (resourceStream == null) {
            return Task.FromException<object?>(new InvalidOperationException($"Null resource stream provided despite contains resource '{rid}'."));
        }
        
        var layout = LayoutExtracting.Extract(resourceStream);

        switch (layout.MajorVersion) {
            case 1: return await ImportV1(rid, resourceStream, layout, importStack);
            default:
                await resourceStream.DisposeAsync();
                return Task.FromException<object?>(new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported."));
        }
    }

    private async Task<object?> ImportV1(ResourceID rid, Stream resourceStream, CompiledResourceLayout layout, IDictionary<ResourceID, Task<object?>> importStack) {
        bool disposeResourceStream = true;
        
        try {
            if (layout.MinorVersion != 0) {
                return Task.FromException<object?>(new NotSupportedException($"Compiled resource version 1.{layout.MinorVersion} is not supported."));
            }
        
            if (!layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out ChunkInformation dataChunkInfo)) {
                return Task.FromException<object?>(new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk."));
            }
        
            if (!layout.TryGetChunkInformation(CompilingConstants.DeserializationChunkTag, out ChunkInformation deserializationChunkInfo)) {
                return Task.FromException<object?>(new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.DeserializationChunkTag)} chunk."));
            }
            
            return await ImportCore(resourceStream, deserializationChunkInfo, dataChunkInfo, importStack);
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

        async Task<object?> ImportCore(Stream resourceStream, ChunkInformation deserializationChunk, ChunkInformation dataChunk, IDictionary<ResourceID, Task<object?>> importStack) {
            resourceStream.Seek(deserializationChunk.ContentOffset, SeekOrigin.Begin);
            
            using BinaryReader reader = new(resourceStream, Encoding.UTF8, true);
            
            string deserializerName = reader.ReadString();
            
            if (!_context.Deserializers.TryGetValue(deserializerName, out Deserializer? deserializer)) {
                throw new ArgumentException($"Deserializer name '{deserializerName}' is unregistered.");
            }
            
            DeserializationContext context;
            object deserialized;
            
            await using (Stream optionsStream = CopyOptionsStream(resourceStream, in layout)) {
                resourceStream.Seek(dataChunk.ContentOffset, SeekOrigin.Begin);
                optionsStream.Seek(0, SeekOrigin.Begin);
            
                await using PartialReadStream dataStream = new(resourceStream, dataChunk.ContentOffset, dataChunk.Length, ownStream: false);
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
            
            Task.WaitAll(importedDependencies.Values.Select(async x => {
                try {
                    return await x;
                } catch (Exception e) {
                    // TODO: Report.
                    return null;
                }
            }));
            
            context.Dependencies = importedDependencies.Where(x => x.Value.IsCompletedSuccessfully).Select(x => KeyValuePair.Create(x.Key, x.Value.Result)).ToDictionary();
            deserializer.ResolveDependencies(deserialized, context);

            return deserialized;
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
        if (_context.Input.ContainResource(rid)) return Task.FromResult<object?>(null);

        throw new NotImplementedException();
    }
}
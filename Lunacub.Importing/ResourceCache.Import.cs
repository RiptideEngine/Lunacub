using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    private async Task<ResourceHandle<T>> ImportSingleResource<T>(ResourceID rid) where T : class {
        if (!_environment.Input.ContainResource(rid)) return new(rid, null);

        Task<object?> importingTask;
        
        await _containerLock.WaitAsync();
        try {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                
                container.ReferenceCount++;
                importingTask = container.FullImportTask;
            } else {
                container = new(rid, 1) {
                    VesselImportTask = ImportResourceVessel(rid, typeof(T)),
                };

                importingTask = container.FullImportTask = FullImport(rid, container.VesselImportTask, [rid]);
                
                _environment.Statistics.IncrementUniqueResourceCount();
            }
            
            _environment.Statistics.IncrementTotalReferenceCount();
        } finally {
            _containerLock.Release();
        }
        
        return new(rid, await importingTask as T);
    }
    
    private ResourceContainer? ImportDependencyResource(ResourceID rid, HashSet<ResourceID> stack) {
        if (!_environment.Input.ContainResource(rid)) return null;

        lock (stack) {
            stack.Add(rid);
        }

        Debug.Assert(_containerLock.CurrentCount == 1);
        
        _containerLock.Wait();
        try {
            ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, rid, out bool exists);

            if (exists) {
                Debug.Assert(container!.ReferenceCount != 0);
                
                container.ReferenceCount++;
                _environment.Statistics.IncrementTotalReferenceCount();
                return container;
            }
            
            container = new(rid, 1) {
                VesselImportTask = ImportResourceVessel(rid, typeof(object)),
            };
            container.FullImportTask = FullImport(rid, container.VesselImportTask, stack);
            
            _environment.Statistics.IncrementTotalReferenceCount();
            _environment.Statistics.IncrementUniqueResourceCount();
            return container;
        } finally {
            _containerLock.Release();
        }
    }
    
    private async Task<ResourceVessel> ImportResourceVessel(ResourceID rid, Type type) {
        await Task.Yield();
        
        Stream? resourceStream = _environment.Input.CreateResourceStream(rid);

        if (resourceStream == null) {
            throw new InvalidOperationException($"Null resource stream provided despite contains resource '{rid}'.");
        }
        
        var layout = LayoutExtracting.Extract(resourceStream);

        switch (layout.MajorVersion) {
            case 1: return ImportResourceVesselV1(type, resourceStream, layout);
                
            default:
                await resourceStream.DisposeAsync();
                throw new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported.");
        }
    }

    private async Task<object?> FullImport(ResourceID rid, Task<ResourceVessel> vesselTask, HashSet<ResourceID> stack) {
        (Deserializer deserializer, object deserialized, DeserializationContext deserializationContext) = await vesselTask;

        if (deserialized == null!) return null;
        
        // TODO: Handle failure and cancelling in the future.
        Debug.Assert(vesselTask.Status == TaskStatus.RanToCompletion);
                
        Dictionary<string, ResourceContainer> importedDependencies = [];
                
        foreach ((string property, DeserializationContext.RequestingDependency requesting) in deserializationContext.RequestingDependencies) {
            if (ImportDependencyResource(requesting.Rid, stack) is not { } dependencyContainer) continue;
            
            importedDependencies.Add(property, dependencyContainer);
        }
        
        await Task.WhenAll(importedDependencies.Values.Select(async x => {
            try {
                return await x.VesselImportTask;
            } catch (Exception e) {
                _environment.Logger.LogError(Logging.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                return default;
            }
        }));
                
        deserializationContext.Dependencies = importedDependencies.Where(x => x.Value.VesselImportTask.IsCompletedSuccessfully).Select(x => KeyValuePair.Create(x.Key, x.Value.VesselImportTask.Result.Deserialized)).Where(x => x.Value != null!).ToDictionary()!;
        deserializer.ResolveDependencies(deserialized, deserializationContext);

        IEnumerable<ResourceContainer> filteredContainers;
        
        lock (stack) {
            filteredContainers = importedDependencies.Values.ExceptBy(stack, c => c.Rid);
        }

        await Task.WhenAll(filteredContainers.Select(async x => {
            try {
                return await x.FullImportTask;
            } catch (Exception e) {
                _environment.Logger.LogError(Logging.DependencyImportExceptionOccuredEvent, e, "Exception occured while importing dependency resource {rid}.", rid);
                return null;
            }
        }));

        return deserialized;
    }
    
    private ResourceVessel ImportResourceVesselV1(Type type, Stream resourceStream, CompiledResourceLayout layout) {
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
            
            DeserializationContext context;
            object deserialized;
            
            using (Stream optionsStream = CopyOptionsStream(resourceStream, in layout)) {
                resourceStream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
                optionsStream.Seek(0, SeekOrigin.Begin);
            
                using PartialReadStream dataStream = new(resourceStream, dataChunkInfo.ContentOffset, dataChunkInfo.Length, ownStream: false);
                context = new();
                
                deserialized = deserializer.DeserializeObject(dataStream, optionsStream, context);
            
                if (deserializer.Streaming) {
                    disposeResourceStream = false;
                }
            }
    
            return new(deserializer, deserialized, context);
        } finally {
            if (disposeResourceStream) resourceStream.Dispose();
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
}
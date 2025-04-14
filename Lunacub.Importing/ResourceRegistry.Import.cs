using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceRegistry {
    private object? ImportInner(ResourceID rid, Type type) {
        if (!_context.Input.ContainResource(rid)) return null;

        Dictionary<ResourceID, object> imported = [];

        return ImportRecursiveStart(rid, type, imported);
    }

    private object? ImportRecursiveStart(ResourceID rid, Type type, Dictionary<ResourceID, object> importedStack) {
        ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, rid);
        
        if (!Unsafe.IsNullRef(ref container)) {
            Debug.Assert(container.ReferenceCount != 0);
                
            container.ReferenceCount++;
            return container.Value;
        }

        return ImportRecursiveBody(rid, type, importedStack);
    }

    private object? ImportRecursiveBody(ResourceID rid, Type type, Dictionary<ResourceID, object> importedStack) {
        if (!_context.Input.ContainResource(rid)) return null;
        
        if (_resourceCache.TryGetValue(rid, out var cachedContainer)) return cachedContainer.Value;
        if (importedStack.TryGetValue(rid, out var imported)) return imported;
        
        Stream? resourceStream = _context.Input.CreateResourceStream(rid);

        if (resourceStream == null) {
            throw new InvalidOperationException($"Null resource stream provided despite contains resource '{rid}'.");
        }

        DetachableDisposable<Stream> detachableStream = new(resourceStream);
        try {
            var layout = LayoutExtracting.Extract(detachableStream.Value!);

            return layout.MajorVersion switch {
                1 => ImportV1(rid, ref detachableStream, type, in layout, importedStack),
                _ => throw new NotSupportedException($"Compiled resource version {layout.MajorVersion}.{layout.MinorVersion} is not supported."),
            };
        } finally {
            detachableStream.Dispose();
        }
    }

    private object? ImportV1(ResourceID rid, ref DetachableDisposable<Stream> resourceStream, Type type, in CompiledResourceLayout layout, Dictionary<ResourceID, object> importedStack) {
        if (layout.MinorVersion != 0) {
            throw new NotSupportedException($"Compiled resource version 1.{layout.MinorVersion} is not supported.");
        }

        if (!layout.TryGetChunkInformation(CompilingConstants.DeserializationChunkTag, out ChunkInformation deserializationChunkInfo)) {
            throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
        }

        resourceStream.Value!.Seek(deserializationChunkInfo.ContentOffset, SeekOrigin.Begin);

        using BinaryReader reader = new(resourceStream.Value!, Encoding.UTF8, true);
        string deserializerName = reader.ReadString();

        if (!_context.Deserializers.TryGetValue(deserializerName, out Deserializer? deserializer)) {
            throw new ArgumentException($"Deserializer name '{deserializerName}' is unregistered.");
        }

        if (!deserializer.OutputType.IsAssignableTo(type)) {
            return null;
        }
        
        if (!layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out ChunkInformation dataChunkInfo)) {
            throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
        }
        
        // Copy options to a MemoryStream.
        Stream optionsStream;

        if (layout.TryGetChunkInformation(CompilingConstants.ImportOptionsChunkTag, out ChunkInformation optionsChunkInfo)) {
            resourceStream.Value!.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
            
            optionsStream = new MemoryStream((int)optionsChunkInfo.Length);
            resourceStream.Value!.CopyTo(optionsStream, (int)optionsChunkInfo.Length, 512);
        } else {
            optionsStream = Stream.Null;
        }

        DeserializationContext context;
        object deserialized;
        
        try {
            resourceStream.Value.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
            optionsStream.Seek(0, SeekOrigin.Begin);

            using PartialReadStream dataStream = new(resourceStream.Value, dataChunkInfo.ContentOffset, dataChunkInfo.Length, ownStream: false);
            context = new();
            deserialized = deserializer.DeserializeObject(dataStream, optionsStream, context);

            if (deserializer.Streaming) {
                resourceStream.Detach();
            }
        } finally {
            optionsStream.Dispose();
        }

        importedStack.Add(rid, deserialized);
        
        Dictionary<string, object?> importedDependencies = [];

        foreach ((string property, DeserializationContext.RequestingDependency requesting) in context.RequestingDependencies) {
            importedDependencies.Add(property, ImportRecursiveStart(requesting.Rid, requesting.Type, importedStack));
        }

        context.Dependencies = importedDependencies;
        deserializer.ResolveDependencies(deserialized, context);

        _resourceCache.Add(rid, new(1, deserialized));
        
        return deserialized;
    }
}
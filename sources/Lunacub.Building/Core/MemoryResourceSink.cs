using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryResourceSink : IResourceSink {
    public LibraryIdentityDictionary<LibraryOutput> Outputs { get; } = [];

    public void FlushCompiledResource(Stream sourceStream, ResourceAddress address) {
        if (!Outputs.TryGetValue(address.LibraryId, out var libraryOutput)) {
            libraryOutput = new([], []);
            Outputs.Add(address.LibraryId, libraryOutput);
        }
        
        byte[] buffer = new byte[sourceStream.Length];
        sourceStream.ReadExactly(buffer, 0, buffer.Length);

        libraryOutput.CompiledResources[address.ResourceId] = ImmutableCollectionsMarshal.AsImmutableArray(buffer);
    }

    public void FlushLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId) {
        if (!Outputs.TryGetValue(libraryId, out var libraryOutput)) return;

        libraryOutput.Registry.Clear();
        
        foreach ((var resourceId, var element) in registry) {
            libraryOutput.Registry.Add(resourceId, element);
        }
    }

    public readonly record struct LibraryOutput(
        Dictionary<ResourceID, ImmutableArray<byte>> CompiledResources,
        ResourceRegistry<ResourceRegistry.Element> Registry
    );
}
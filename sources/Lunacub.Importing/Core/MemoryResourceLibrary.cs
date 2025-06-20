using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemoryResourceLibrary : ResourceLibrary {
    public override ResourceRegistry Registry { get; }
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; }

    public MemoryResourceLibrary(ResourceRegistry registry) {
        Resources = [];
        Registry = registry;
    }
    
    public MemoryResourceLibrary(ResourceRegistry registry, IEnumerable<KeyValuePair<ResourceID, ImmutableArray<byte>>> resources) {
        Resources = new(resources);
        Registry = registry;
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        if (!Resources.TryGetValue(rid, out var buffer)) return null;

        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
}
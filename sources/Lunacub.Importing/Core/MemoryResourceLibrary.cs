using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemoryResourceLibrary : ImportResourceLibrary {
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; }

    public MemoryResourceLibrary() {
        Resources = [];
    }
    
    public MemoryResourceLibrary(IEnumerable<KeyValuePair<ResourceID, ImmutableArray<byte>>> resources) {
        Resources = new(resources);
    }

    protected override Stream? CreateResourceStreamCore(ResourceID rid, PrimitiveRegistryElement element) {
        if (!Resources.TryGetValue(rid, out var buffer)) return null;

        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
}
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemoryResourceLibrary : ResourceLibrary {
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; }

    public MemoryResourceLibrary() {
        Resources = [];
    }
    
    public MemoryResourceLibrary(IEnumerable<KeyValuePair<ResourceID, ImmutableArray<byte>>> resources) {
        Resources = new(resources);
    }

    public override bool Contains(ResourceID rid) => Resources.ContainsKey(rid);
    
    protected override Stream? CreateStreamImpl(ResourceID rid) {
        if (!Resources.TryGetValue(rid, out var buffer)) return null;

        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
    
    public override IEnumerator<ResourceID> GetEnumerator() => Resources.Keys.GetEnumerator();
}
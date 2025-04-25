using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemoryResourceLibrary : ResourceLibrary {
    public IReadOnlyDictionary<ResourceID, ImmutableArray<byte>> CompiledResources { get; }
    
    public MemoryResourceLibrary(Guid id, IReadOnlyDictionary<ResourceID, ImmutableArray<byte>> compiledResources) : base(id) {
        CompiledResources = compiledResources;
    }

    public override bool Contains(ResourceID rid) => CompiledResources.ContainsKey(rid);
    
    protected override Stream? CreateStreamImpl(ResourceID rid) {
        if (!CompiledResources.TryGetValue(rid, out var buffer)) return null;

        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
    
    public override IEnumerator<ResourceID> GetEnumerator() => CompiledResources.Keys.GetEnumerator();
}
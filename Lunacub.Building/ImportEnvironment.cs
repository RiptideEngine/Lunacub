namespace Caxivitual.Lunacub.Building;

public sealed class ImportingContext {
    internal List<ResourceReference> References { get; }
    
    internal ImportingContext() {
        References = [];
    }

    public void SetReference(ResourceID rid, ResourceReferenceType type) {
        if (rid == ResourceID.Null) return;

        foreach (ref var reference in CollectionsMarshal.AsSpan(References)) {
            if (reference.Rid == rid) {
                reference = new(rid, type);
                return;
            }
        }
        
        References.Add(new(rid, type));
    }

    public readonly record struct ResourceReference(ResourceID Rid, ResourceReferenceType Type);
}
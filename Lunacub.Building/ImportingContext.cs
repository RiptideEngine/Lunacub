namespace Caxivitual.Lunacub.Building;

public sealed class ImportingContext {
    internal List<ResourceID> References { get; }
    
    public object? Options { get; }
    
    internal ImportingContext(object? options) {
        References = [];
        Options = options;
    }

    public void AddReference(ResourceID rid) {
        if (References.Contains(rid)) return;
        
        References.Add(rid);
    }

    public void RemoveReference(ResourceID rid) {
        References.Remove(rid);
    }
}
namespace Caxivitual.Lunacub.Building;

public sealed class ImportingContext {
    internal ICollection<ResourceID> References { get; }
    
    public IImportOptions? Options { get; }
    
    internal ImportingContext(IImportOptions? options) {
        References = new List<ResourceID>();    // A HashSet work too but I'm trying to keep things slim here.
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
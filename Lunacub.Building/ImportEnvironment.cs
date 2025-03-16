namespace Caxivitual.Lunacub.Building;

public sealed class ImportingContext {
    public HashSet<ResourceID> Dependencies { get; }
    
    internal ImportingContext() {
        Dependencies = [];
    }
}
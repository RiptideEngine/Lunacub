namespace Caxivitual.Lunacub.Building;

public abstract class ContentRepresentation {
    // TODO: Specialized collection type to lock edit after Import step.
    public HashSet<ResourceID> Dependencies { get; }

    protected ContentRepresentation() {
        Dependencies = [];
    }
}
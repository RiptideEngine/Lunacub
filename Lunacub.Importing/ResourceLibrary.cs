using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

public abstract class ResourceLibrary {
    public Guid Id { get; }
    
    protected ResourceLibrary(Guid id) {
        Id = id;
    }

    public abstract bool Contains(ResourceID rid);
    public abstract Stream CreateStream(ResourceID rid);
}
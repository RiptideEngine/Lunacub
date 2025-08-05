namespace Caxivitual.Lunacub.Building.Collections;

public sealed class ResourceLibraryCollection : ResourceLibraryCollection<BuildResourceLibrary> {
    internal ResourceLibraryCollection() {
    }
    
    public bool ContainsResource(ResourceID resourceId) {
        foreach (var library in this) {
            if (library.Registry.ContainsKey(resourceId)) return true;
        }

        return false;
    }

    // TODO: Validate name collision.
}
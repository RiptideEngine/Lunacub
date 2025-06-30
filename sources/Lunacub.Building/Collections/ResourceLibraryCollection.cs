namespace Caxivitual.Lunacub.Building.Collections;

public sealed class ResourceLibraryCollection : Collection<BuildResourceLibrary> {
    internal ResourceLibraryCollection() {
    }
    
    public bool ContainsResource(ResourceID resourceId) {
        foreach (var library in this) {
            if (library.Registry.ContainsKey(resourceId)) return true;
        }

        return false;
    }
    
    protected override void InsertItem(int index, BuildResourceLibrary item) {
        ValidateAppendingLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, BuildResourceLibrary item) {
        ValidateAppendingLibrary(item);
        
        base.SetItem(index, item);
    }

    private void ValidateAppendingLibrary(BuildResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item);
        
        // TODO: Validate ID, name collision.
    }
}
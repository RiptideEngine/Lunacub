using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing.Collections;

public sealed class ResourceLibraryCollection : Collection<ResourceLibrary> {
    public bool ContainResource(ResourceID rid) {
        foreach (var library in this) {
            if (library.Registry.ContainsKey(rid)) return true;
        }
        
        return false;
    }

    public Stream? CreateResourceStream(ResourceID rid) {
        foreach (var library in this) {
            if (library.CreateStream(rid) is { } stream) return stream;
        }

        return null;
    }
    
    protected override void InsertItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        if (!ReferenceEquals(item, this[index])) {
            base.SetItem(index, item);
        }
    }
}
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
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ResourceLibrary item) {
        ValidateLibrary(item);

        if (!ReferenceEquals(item, this[index])) {
            base.SetItem(index, item);
        }
    }

    [StackTraceHidden]
    private void ValidateLibrary(ResourceLibrary library) {
        ArgumentNullException.ThrowIfNull(library);
        
        foreach (var insertedLibrary in this) {
            ResourceRegistry insertedLibraryRegistry = insertedLibrary.Registry;
            
            foreach (var libraryRegistryId in library.Registry.Keys) {
                if (insertedLibraryRegistry.ContainsKey(libraryRegistryId)) {
                    throw new ArgumentException("");
                }
            }
        }
    }
}
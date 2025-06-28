using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub;

public class ResourceLibraryCollection<TLibrary, TElement> : Collection<TLibrary> where TLibrary : ResourceLibrary<TElement> {
    public bool ContainResource(ResourceID rid) {
        foreach (var library in this) {
            if (library.Registry.ContainsKey(rid)) return true;
        }
        
        return false;
    }

    public Stream? CreateResourceStream(ResourceID rid) {
        foreach (var library in this) {
            if (library.CreateResourceStream(rid) is { } stream) return stream;
        }

        return null;
    }
    
    protected override void InsertItem(int index, TLibrary item) {
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, TLibrary item) {
        ValidateLibrary(item);

        if (!ReferenceEquals(item, this[index])) {
            base.SetItem(index, item);
        }
    }

    [StackTraceHidden]
    private void ValidateLibrary(TLibrary library) {
        ArgumentNullException.ThrowIfNull(library);
        
        foreach (var insertedLibrary in this) {
            ResourceRegistry<TElement> insertedLibraryRegistry = insertedLibrary.Registry;
            
            foreach (var libraryResourceIds in library.Registry.Keys) {
                if (insertedLibraryRegistry.ContainsKey(libraryResourceIds)) {
                    throw new ArgumentException($"Collision of ResourceId {libraryResourceIds} detected.");
                }
            }
        }
    }
}
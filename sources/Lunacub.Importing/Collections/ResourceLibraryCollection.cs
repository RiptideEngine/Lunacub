using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing.Collections;

public sealed class ResourceLibraryCollection<TElement> : Collection<ResourceLibrary<TElement>> where TElement : IRegistryElement {
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
    
    protected override void InsertItem(int index, ResourceLibrary<TElement> item) {
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ResourceLibrary<TElement> item) {
        ValidateLibrary(item);

        if (!ReferenceEquals(item, this[index])) {
            base.SetItem(index, item);
        }
    }

    [StackTraceHidden]
    private void ValidateLibrary(ResourceLibrary<TElement> library) {
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
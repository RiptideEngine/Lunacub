using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing.Collections;

public sealed class ResourceLibraryCollection : Collection<ImportResourceLibrary> {
    public bool Contains(LibraryID libraryId) {
        foreach (var library in this) {
            if (library.Id == libraryId) return true;
        }

        return false;
    }
    
    public bool Contains(LibraryID libraryId, [NotNullWhen(true)] out ImportResourceLibrary? output) {
        foreach (var library in this) {
            if (library.Id == libraryId) {
                output = library;
                return true;
            }
        }

        output = null;
        return false;
    }

    public bool ContainsResource(LibraryID libraryId, ResourceID resourceId) {
        foreach (var library in this) {
            if (library.Id == libraryId) {
                return library.Registry.ContainsKey(resourceId);
            }
        }

        return false;
    }
    
    public bool ContainsResource(ResourceAddress address) {
        return ContainsResource(address.LibraryId, address.ResourceId);
    }
    
    public bool ContainsResource(LibraryID libraryId, ResourceID resourceId, out ResourceRegistry.Element output) {
        foreach (var library in this) {
            if (library.Id != libraryId) continue;
            
            if (library.Registry.TryGetValue(resourceId, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public bool ContainsResource(ResourceAddress address, out ResourceRegistry.Element output) {
        return ContainsResource(address.LibraryId, address.ResourceId, out output);
    }
    
    public bool ContainsResource(LibraryID libraryId, ReadOnlySpan<char> name) {
        foreach (var library in this) {
            if (library.Id != libraryId) continue;
            
            if (library.Registry.ContainsName(name)) return true;
        }

        return false;
    }
    
    public bool ContainsResource(LibraryID libraryId, ReadOnlySpan<char> name, out ResourceID output) {
        foreach (var library in this) {
            if (library.Id != libraryId) continue;
            
            if (library.Registry.TryGetValue(name, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public bool ContainsResource(LibraryID libraryId, ReadOnlySpan<char> name, out ResourceRegistry.Element output) {
        foreach (var library in this) {
            if (library.Id != libraryId) continue;
            
            if (library.Registry.TryGetValue(name, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public Stream? CreateResourceStream(LibraryID libraryId, ResourceID resourceId) {
        foreach (var library in this) {
            if (library.Id != libraryId) continue;
            
            if (library.CreateResourceStream(resourceId) is { } stream) return stream;
        }

        return null;
    }
    
    public Stream? CreateResourceStream(ResourceAddress address) {
        return CreateResourceStream(address.LibraryId, address.ResourceId);
    }
    
    protected override void InsertItem(int index, ImportResourceLibrary item) {
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ImportResourceLibrary item) {
        ValidateLibrary(item);
        
        base.SetItem(index, item);
    }

    private void ValidateLibrary(ImportResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item);
        
        // TODO: Validate ID, name collision.
    }
}
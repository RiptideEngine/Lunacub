namespace Caxivitual.Lunacub.Importing.Collections;

public sealed class ResourceLibraryCollection : ResourceLibraryCollection<ImportResourceLibrary> {
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

    // TODO: Validate name collision.
}
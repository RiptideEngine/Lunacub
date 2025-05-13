using Caxivitual.Lunacub.Importing.Collections;

namespace Caxivitual.Lunacub.Importing;

public sealed class InputSystem {
    public ResourceLibraryCollection Libraries { get; }

    internal InputSystem() {
        Libraries = [];
    }

    public bool ContainResource(ResourceID rid) {
        foreach (var library in Libraries) {
            if (library.Contains(rid)) return true;
        }
        
        return false;
    }

    public Stream? CreateResourceStream(ResourceID rid) {
        foreach (var library in Libraries) {
            if (library.CreateStream(rid) is { } stream) return stream;
        }

        return null;
    }
}
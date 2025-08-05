using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub;

public class ResourceLibraryCollection<T> : Collection<T> where T : ResourceLibrary {
    public bool Contains(LibraryID libraryId) {
        foreach (var library in this) {
            if (library.Id == libraryId) return true;
        }

        return false;
    }
    
    public bool Contains(LibraryID libraryId, [NotNullWhen(true)] out T? output) {
        foreach (var library in this) {
            if (library.Id == libraryId) {
                output = library;
                return true;
            }
        }

        output = null;
        return false;
    }
    
    protected sealed override void InsertItem(int index, T item) {
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected sealed override void SetItem(int index, T item) {
        ValidateLibrary(item);
        
        base.SetItem(index, item);
    }

    protected virtual void ValidateLibrary(T item, [CallerArgumentExpression(nameof(item))] string? paramName = null) {
        Debug.Assert(item.Id != LibraryID.Null);
        
        ArgumentNullException.ThrowIfNull(item);

        foreach (var library in this) {
            if (library.Id == item.Id) {
                string message = string.Format(ExceptionMessages.DuplicateLibraryId, item.Id);
                throw new ArgumentException(message, paramName);
            }
        }
        
        // TODO: Validate ID, name collision.
    }
}
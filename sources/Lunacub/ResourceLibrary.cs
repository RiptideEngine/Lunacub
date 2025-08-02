namespace Caxivitual.Lunacub;

public abstract class ResourceLibrary {
    public LibraryID Id { get; }

    protected ResourceLibrary(LibraryID id) {
        if (id == LibraryID.Null) {
            throw new ArgumentException(ExceptionMessages.LibraryIDMustBeNotNull, nameof(id));
        }
        
        Id = id;
    }
}
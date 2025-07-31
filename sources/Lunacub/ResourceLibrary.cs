namespace Caxivitual.Lunacub;

public abstract class ResourceLibrary {
    public LibraryID Id { get; }

    protected ResourceLibrary(LibraryID id) {
        Id = id;
    }
}
namespace Caxivitual.Lunacub.Importing;

public sealed class ImportResourceLibrary : ResourceLibrary {
    private readonly SourceRepository _provider;
    
    public ResourceRegistry<ResourceRegistry.Element> Registry { get; }
    
    public ImportResourceLibrary(LibraryID id, SourceRepository sourceRepository) : base(id) {
        ArgumentNullException.ThrowIfNull(sourceRepository, nameof(sourceRepository));
        
        _provider = sourceRepository;
        Registry = [];
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (!Registry.ContainsKey(resourceId)) return null;
        
        return _provider.CreateStream(resourceId);
    }
}
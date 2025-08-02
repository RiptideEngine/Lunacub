namespace Caxivitual.Lunacub.Importing;

public sealed class ImportResourceLibrary : ResourceLibrary {
    private readonly ImportSourceProvider _provider;
    
    public ResourceRegistry<ResourceRegistry.Element> Registry { get; }
    
    public ImportResourceLibrary(LibraryID id, ImportSourceProvider sourceProvider) : base(id) {
        ArgumentNullException.ThrowIfNull(sourceProvider, nameof(sourceProvider));
        
        _provider = sourceProvider;
        Registry = [];
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (!Registry.ContainsKey(resourceId)) return null;
        
        return _provider.CreateStream(new(Id, resourceId));
    }
}
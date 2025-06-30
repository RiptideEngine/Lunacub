namespace Caxivitual.Lunacub.Importing;

public sealed class ImportResourceLibrary {
    private SourceProvider _provider;
    
    public ResourceRegistry<byte> Registry { get; }
    
    public ImportResourceLibrary(SourceProvider sourceProvider) {
        _provider = sourceProvider;
        Registry = [];
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (resourceId == ResourceID.Null || !Registry.ContainsKey(resourceId)) return null;
        
        return _provider.CreateStream(resourceId);
    }
}
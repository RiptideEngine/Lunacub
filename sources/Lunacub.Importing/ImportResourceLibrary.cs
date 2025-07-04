﻿namespace Caxivitual.Lunacub.Importing;

public sealed class ImportResourceLibrary {
    private ImportSourceProvider _provider;
    
    public ResourceRegistry<byte> Registry { get; }
    
    public ImportResourceLibrary(ImportSourceProvider sourceProvider) {
        _provider = sourceProvider;
        Registry = [];
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (resourceId == ResourceID.Null || !Registry.ContainsKey(resourceId)) return null;
        
        return _provider.CreateStream(resourceId);
    }
}
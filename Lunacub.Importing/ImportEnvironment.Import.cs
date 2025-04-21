namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceCache _resourceCache;

    public ImportingOperation ImportAsync(ResourceID rid) {
        return new(rid, _resourceCache.ImportAsync(rid));
    }

    public Task<ResourceHandle<T>> ImportAsync<T>(ResourceID rid) where T : class {
        return _resourceCache.ImportAsync<T>(rid);
    }
    
    public ReleaseStatus Release(object? resource) {
        if (resource is null) return ReleaseStatus.Null;
        
        return _resourceCache.Release(resource);
    }

    public ReleaseStatus Release(ImportingOperation operation) {
        if (operation.Rid == ResourceID.Null) return ReleaseStatus.Null;

        return _resourceCache.Release(operation.Rid);
    }
    
    public ReleaseStatus Release(ResourceHandle handle) {
        if (handle.Rid == ResourceID.Null) return ReleaseStatus.Null;

        return _resourceCache.Release(handle.Rid);
    }
}
namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceCache _resourceCache;

    public ImportingOperation ImportAsync(ResourceID rid) {
        return _resourceCache.ImportAsync(rid);
    }

    public ImportingOperation<T> ImportAsync<T>(ResourceID rid) where T : class {
        return _resourceCache.ImportAsync<T>(rid);
    }

    public ReleaseStatus Release(object? resource) {
        if (resource is null) return ReleaseStatus.Null;
        
        return _resourceCache.Release(resource);
    }
    
    public ReleaseStatus Release(ResourceHandle handle) {
        if (handle.Rid == ResourceID.Null || handle.Value == null) return ReleaseStatus.Null;

        return _resourceCache.Release(handle);
    }

    public ReleaseStatus Release(ResourceID rid) {
        if (rid == ResourceID.Null) return ReleaseStatus.Null;
        
        return _resourceCache.Release(rid);
    }
}
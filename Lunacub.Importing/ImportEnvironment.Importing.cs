namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceCache _resourceCache;

    public Task<ResourceHandle> ImportAsync(ResourceID rid) {
        return _resourceCache.ImportAsync(rid);
    }

    public Task<ResourceHandle<T>> ImportAsync<T>(ResourceID rid) where T : class {
        return _resourceCache.ImportAsync<T>(rid);
    }
    
    public ReleaseStatus Release(object? resource) {
        throw new NotImplementedException();
        // if (resource is null) return ReleaseStatus.Null;
        //
        // return _resourceCache.Release(resource);
    }
}
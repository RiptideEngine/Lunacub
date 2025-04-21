namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceCache : IDisposable {
    private readonly Lock _cacheLock;
    private readonly Dictionary<ResourceID, CacheContainer> _cacheDict;
    private readonly ImportEnvironment _environment;
    
    private bool _disposed;

    internal ResourceCache(ImportEnvironment environment) {
        _cacheLock =  new();
        _cacheDict = [];
        _environment = environment;
    }

    public async Task<ResourceHandle> ImportAsync(ResourceID rid) {
        object? output = await ImportSingleResource(rid);
        return new(rid, output);
    }
    
    public async Task<ResourceHandle<T>> ImportAsync<T>(ResourceID rid) where T : class {
        T? output = await ImportSingleResource(rid, typeof(T)) as T;
        return new(rid, output);
    }

    public ReleaseStatus Release(object? resource) {
        throw new NotImplementedException();
    }
    
    public ReleaseStatus Release(ResourceID rid) {
        throw new NotImplementedException();
    }
    
    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            // using (_lock.EnterScope()) {
            //     foreach ((_, var container) in _resourceCache) {
            //         _context.Disposers.TryDispose(container.Value);
            //     }
            //     
            //     _resourceCache.Clear();
            // }
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~ResourceCache() {
        Dispose(false);
    }

    private class CacheContainer {
        public readonly Task<object?> Task;
        public uint ReferenceCount;
        public CancellationTokenSource CancellationTokenSource;

        public CacheContainer(Task<object?> task, uint initialReferenceCount, CancellationTokenSource cancellationTokenSource) {
            Task = task;
            ReferenceCount = initialReferenceCount;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
}
namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceCache : IDisposable {
    private readonly Lock _cacheLock;
    private readonly Dictionary<ResourceID, CacheContainer> _cacheDict;
    private readonly ImportEnvironment _context;
    
    private bool _disposed;

    internal ResourceCache(ImportEnvironment context) {
        _cacheLock =  new();
        _cacheDict = [];
        _context = context;
    }

    public async Task<ResourceHandle> ImportAsync(ResourceID rid) {
        object? output = await ImportSingleResource(rid);
        return new(rid, output);
    }
    
    public async Task<ResourceHandle<T>> ImportAsync<T>(ResourceID rid) where T : class {
        T? output = await ImportSingleResource(rid, typeof(T)) as T;
        return new(rid, output);
    }
    
    // internal ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
    //     using (_lock.EnterScope()) {
    //         return new(rid, ImportSingleResource(rid, typeof(T)) as T);
    //     }
    // }
    //
    // internal ReleaseStatus Release(ResourceHandle handle) {
    //     if (handle.Value is not { } releasingInstance) return ReleaseStatus.ResourceIncompatible;
    //
    //     using (_lock.EnterScope()) {
    //         ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, handle.Rid);
    //
    //         if (Unsafe.IsNullRef(ref container)) return ReleaseStatus.ResourceNotImported;
    //         
    //         Debug.Assert(container.ReferenceCount != 0);
    //         
    //         if (!container.Value.Equals(releasingInstance)) return ReleaseStatus.ResourceIncompatible;
    //         
    //         if (--container.ReferenceCount == 0) {
    //             bool result = _context.Disposers.TryDispose(releasingInstance);
    //             _resourceCache.Remove(handle.Rid);
    //             return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
    //         }
    //
    //         return ReleaseStatus.Success;
    //     }
    // }
    //
    // internal ReleaseStatus Release(ResourceID rid) {
    //     using (_lock.EnterScope()) {
    //         ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, rid);
    //
    //         if (Unsafe.IsNullRef(ref container)) return ReleaseStatus.ResourceNotImported;
    //         
    //         Debug.Assert(container.ReferenceCount != 0);
    //         
    //         if (--container.ReferenceCount == 0) {
    //             bool result = _context.Disposers.TryDispose(container.Value);
    //             _resourceCache.Remove(rid);
    //             return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
    //         }
    //
    //         return ReleaseStatus.Success;
    //     }
    // }
    
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

        public CacheContainer(Task<object?> task, uint initialReferenceCount = 1) {
            Task = task;
            ReferenceCount = initialReferenceCount;
        }
    }
}
namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceRegistry : IDisposable {
    private readonly ReaderWriterLockSlim _lock;
    private readonly Dictionary<ResourceID, Container> _resourceCache;
    private readonly ImportEnvironment _context;
    
    private bool _disposed;

    internal ResourceRegistry(ImportEnvironment context) {
        _lock =  new(LockRecursionPolicy.SupportsRecursion);
        _resourceCache = [];
        _context = context;
    }
    
    internal ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
        _lock.EnterUpgradeableReadLock();
        try {
            if (_resourceCache.TryGetValue(rid, out var cachedContainer) && cachedContainer.ReferenceCount > 0) {
                return new(rid, cachedContainer.Value as T);
            }
            
            _lock.EnterWriteLock();
            try {
                return new(rid, ImportInner(rid, typeof(T)) as T);
            } finally {
                _lock.ExitWriteLock();
            }
        } finally {
            _lock.ExitUpgradeableReadLock();
        }
    }
    
    internal ReleaseStatus Release(ResourceHandle handle) {
        if (handle.Value is not { } releasingInstance) return ReleaseStatus.ResourceIncompatible;
        
        _lock.EnterUpgradeableReadLock();
        try {
            if (!_resourceCache.TryGetValue(handle.Rid, out Container resourceContainer)) return ReleaseStatus.ResourceNotFound;
            Debug.Assert(resourceContainer.ReferenceCount != 0);

            if (!resourceContainer.Value.Equals(releasingInstance)) return ReleaseStatus.ResourceIncompatible;
            
            _lock.EnterWriteLock();
            try {
                ref var reference = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, handle.Rid);
                Debug.Assert(!Unsafe.IsNullRef(ref reference));

                if (--reference.ReferenceCount == 0) {
                    bool result = _context.Disposers.TryDispose(releasingInstance);
                    _resourceCache.Remove(handle.Rid);

                    return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
                }

                return ReleaseStatus.Success;
            } finally {
                _lock.ExitWriteLock();
            }
        } finally {
            _lock.ExitUpgradeableReadLock();
        }
    }

    internal ReleaseStatus Release(ResourceID rid) {
        _lock.EnterUpgradeableReadLock();
        try {
            if (!_resourceCache.TryGetValue(rid, out Container resourceContainer)) return ReleaseStatus.ResourceNotFound;
            Debug.Assert(resourceContainer.ReferenceCount != 0);
            
            _lock.EnterWriteLock();
            try {
                ref var reference = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, rid);
                Debug.Assert(!Unsafe.IsNullRef(ref reference));

                if (--reference.ReferenceCount == 0) {
                    bool result = _context.Disposers.TryDispose(resourceContainer.Value);
                    _resourceCache.Remove(rid);

                    return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
                }

                return ReleaseStatus.Success;
            } finally {
                _lock.ExitWriteLock();
            }
        } finally {
            _lock.ExitUpgradeableReadLock();
        }
    }
    
    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            // TODO: Dispose Resources.
            
            _lock.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceRegistry() {
        Dispose(false);
    }

    private record struct Container(uint ReferenceCount, object Value);
}
namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceRegistry : IDisposable {
    private readonly Lock _lock;
    private readonly Dictionary<ResourceID, Container> _resourceCache;
    private readonly ImportEnvironment _context;
    
    private bool _disposed;

    internal ResourceRegistry(ImportEnvironment context) {
        _lock =  new();
        _resourceCache = [];
        _context = context;
    }
    
    internal ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
        using (_lock.EnterScope()) {
            return new(rid, ImportSingleResource(rid, typeof(T)) as T);
        }
    }

    internal void ImportFromTags(string query, ICollection<ResourceHandle> outputList) {
        TagFilter filter = new(query);
        
        using (_lock.EnterScope()) {
            foreach (var library in _context.Input.Libraries) {
                foreach (var rid in library) {
                    if (ImportSingleResourceWithTagFilter(rid, filter) is not { } imported) continue;
                    
                    outputList.Add(new(rid, imported));
                }
            }
        }
    }
    
    internal ReleaseStatus Release(ResourceHandle handle) {
        if (handle.Value is not { } releasingInstance) return ReleaseStatus.ResourceIncompatible;

        using (_lock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, handle.Rid);

            if (Unsafe.IsNullRef(ref container)) return ReleaseStatus.ResourceNotImported;
            
            Debug.Assert(container.ReferenceCount != 0);
            
            if (!container.Value.Equals(releasingInstance)) return ReleaseStatus.ResourceIncompatible;
            
            if (--container.ReferenceCount == 0) {
                bool result = _context.Disposers.TryDispose(releasingInstance);
                _resourceCache.Remove(handle.Rid);
                return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
            }

            return ReleaseStatus.Success;
        }
    }

    internal ReleaseStatus Release(ResourceID rid) {
        using (_lock.EnterScope()) {
            ref var container = ref CollectionsMarshal.GetValueRefOrNullRef(_resourceCache, rid);

            if (Unsafe.IsNullRef(ref container)) return ReleaseStatus.ResourceNotImported;
            
            Debug.Assert(container.ReferenceCount != 0);
            
            if (--container.ReferenceCount == 0) {
                bool result = _context.Disposers.TryDispose(container.Value);
                _resourceCache.Remove(rid);
                return result ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
            }

            return ReleaseStatus.Success;
        }
    }
    
    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            using (_lock.EnterScope()) {
                foreach ((_, var container) in _resourceCache) {
                    _context.Disposers.TryDispose(container.Value);
                }
                
                _resourceCache.Clear();
            }
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~ResourceRegistry() {
        Dispose(false);
    }

    private record struct Container(uint ReferenceCount, object Value);
}
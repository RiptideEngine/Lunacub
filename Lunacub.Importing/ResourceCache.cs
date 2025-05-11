namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceCache : IDisposable {
    private readonly SemaphoreSlim _containerLock;
    private readonly Dictionary<ResourceID, ResourceContainer> _resourceContainers;
    private readonly Dictionary<object, ResourceContainer> _importedReleaseCache;
    private readonly ImportEnvironment _environment;
    
    private bool _disposed;

    internal ResourceCache(ImportEnvironment environment) {
        _containerLock =  new(1, 1);
        _resourceContainers = [];
        _importedReleaseCache = [];
        _environment = environment;
    }

    public ImportingOperation ImportAsync(ResourceID rid) {
        return new(rid, ImportSingleResource(rid));
    }

    public ImportingOperation<T> ImportAsync<T>(ResourceID rid) where T : class {
        return new(rid, ImportSingleResource<T>(rid));
    }

    public ReleaseStatus Release(object resource) {
        _containerLock.Wait();
        try {
            if (!_importedReleaseCache.TryGetValue(resource, out var container)) return ReleaseStatus.ResourceNotImported;
            
            Debug.Assert(container.FullImportTask.Status == TaskStatus.RanToCompletion);
            Debug.Assert(ReferenceEquals(container.FullImportTask.Result, resource));

            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) {
                return ReleaseStatus.Success;
            }

            _environment.Statistics.DecrementUniqueResourceCount();

            bool removal = _importedReleaseCache.Remove(resource);
            Debug.Assert(removal);
        
            removal = _resourceContainers.Remove(container.Rid);
            Debug.Assert(removal);

            if (_environment.Disposers.TryDispose(resource)) {
                _environment.Statistics.IncrementDisposedResourceCount();
                return ReleaseStatus.Success;
            }

            _environment.Statistics.IncrementUndisposedResourceCount();
            return ReleaseStatus.NotDisposed;
        } finally {
            _containerLock.Release();
        }
    }
    
    public ReleaseStatus Release(ResourceHandle rid) {
        _containerLock.Wait();
        try {
            if (!_importedReleaseCache.TryGetValue(rid.Value!, out var container)) return ReleaseStatus.ResourceNotImported;
            if (container.Rid != rid.Rid) return ReleaseStatus.ResourceIncompatible;
            
            Debug.Assert(container.FullImportTask.Status == TaskStatus.RanToCompletion);
            Debug.Assert(ReferenceEquals(container.FullImportTask.Result, rid.Value));
            
            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) return ReleaseStatus.Success;

            object releasedResource = container.FullImportTask.Result!;
        
            _environment.Statistics.DecrementUniqueResourceCount();

            bool removal = _importedReleaseCache.Remove(releasedResource);
            Debug.Assert(removal);
        
            removal = _resourceContainers.Remove(container.Rid);
            Debug.Assert(removal);

            if (_environment.Disposers.TryDispose(releasedResource)) {
                _environment.Statistics.IncrementDisposedResourceCount();
                return ReleaseStatus.Success;
            }

            _environment.Statistics.IncrementUndisposedResourceCount();
            return ReleaseStatus.NotDisposed;
        } finally {
            _containerLock.Release();
        }
    }

    public ReleaseStatus Release(ResourceID rid) {
        _containerLock.Wait();
        try {
            if (!_resourceContainers.TryGetValue(rid, out var container)) return ReleaseStatus.ResourceNotImported;
            
            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) return ReleaseStatus.Success;

            Debug.Assert(_containerLock.CurrentCount == 0);
            Debug.Assert(container.ReferenceCount == 0);
        
            container.CancellationTokenSource.Cancel();

            try {
                container.FullImportTask.Wait();
            } catch (AggregateException ae) {
                foreach (var e in ae.InnerExceptions) {
                    if (e is TaskCanceledException or OperationCanceledException) continue;
                
                    // TODO: Report
                }
            }

            container.CancellationTokenSource.Dispose();

            switch (container.FullImportTask.Status) {
                case TaskStatus.RanToCompletion:
                    object releasedResource = container.FullImportTask.Result!;
        
                    _environment.Statistics.DecrementUniqueResourceCount();

                    bool removal = _importedReleaseCache.Remove(releasedResource);
                    Debug.Assert(removal);
        
                    removal = _resourceContainers.Remove(container.Rid);
                    Debug.Assert(removal);

                    if (_environment.Disposers.TryDispose(releasedResource)) {
                        _environment.Statistics.IncrementDisposedResourceCount();
                        return ReleaseStatus.Success;
                    }

                    _environment.Statistics.IncrementUndisposedResourceCount();
                    return ReleaseStatus.NotDisposed;
            
                case TaskStatus.Canceled or TaskStatus.Faulted:
                    Debug.Assert(!_resourceContainers.ContainsKey(container.Rid));
                    return ReleaseStatus.Canceled;
            
                default: throw new UnreachableException($"Unexpected task status '{container.FullImportTask.Status}'.");
            }
        } finally {
            _containerLock.Release();
        }
    }

    private uint DecrementResourceContainerReference(ref uint referenceCount) {
        _environment.Statistics.Release();

        return --referenceCount;
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            _containerLock.Dispose();
            
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
}
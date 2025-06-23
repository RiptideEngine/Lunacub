using System.Collections.Concurrent;

namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceCache : IDisposable, IAsyncDisposable {
    private readonly SemaphoreSlim _containerLock;
    private readonly Dictionary<ResourceID, ResourceContainer> _resourceContainers;
    private readonly ConcurrentDictionary<object, ResourceContainer> _importedObjectMap;
    private readonly ImportEnvironment _environment;
    
    private bool _disposed;

    internal ResourceCache(ImportEnvironment environment) {
        _containerLock =  new(1, 1);
        _resourceContainers = [];
        _importedObjectMap = [];
        _environment = environment;
    }

    public ImportingOperation ImportAsync(ResourceID resourceId) {
        return new(resourceId, ImportSingleResource(resourceId));
    }

    public ImportingOperation<T> ImportAsync<T>(ResourceID resourceId) where T : class {
        return new(resourceId, ImportSingleResource<T>(resourceId));
    }

    public ReleaseStatus Release(object resource) {
        _containerLock.Wait();
        try {
            if (!_importedObjectMap.TryGetValue(resource, out ResourceContainer? container)) return ReleaseStatus.InvalidResource;
            
            if (container.ReferenceWaitTask == null) return ReleaseStatus.NotImported;
            
            Debug.Assert(container.ReferenceWaitTask.Status == TaskStatus.RanToCompletion);
            Debug.Assert(ReferenceEquals(container.ReferenceWaitTask.Result, resource));

            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) {
                return ReleaseStatus.Success;
            }

            _environment.Statistics.DecrementUniqueResourceCount();

            bool removal = _importedObjectMap.TryRemove(resource, out _);
            Debug.Assert(removal);

            removal = _resourceContainers.Remove(container.ResourceId);
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
    
    public ReleaseStatus Release(ResourceHandle resourceId) {
        _containerLock.Wait();
        try {
            if (!_importedObjectMap.TryGetValue(resourceId.Value!, out var container)) return ReleaseStatus.InvalidResource;
            if (container.ResourceId != resourceId.Rid) return ReleaseStatus.IdIncompatible;
            
            Debug.Assert(container.ReferenceWaitTask!.Status == TaskStatus.RanToCompletion);
            Debug.Assert(ReferenceEquals(container.ReferenceWaitTask.Result, resourceId.Value));
            
            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) return ReleaseStatus.Success;
            
            object releasedResource = container.ReferenceWaitTask.Result;
            
            _environment.Statistics.DecrementUniqueResourceCount();
            
            bool removal = _importedObjectMap.TryRemove(releasedResource, out _);
            Debug.Assert(removal);
            
            removal = _resourceContainers.Remove(container.ResourceId);
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
            if (!_resourceContainers.TryGetValue(rid, out var container)) return ReleaseStatus.NotImported;
            
            if (DecrementResourceContainerReference(ref container.ReferenceCount) != 0) return ReleaseStatus.Success;

            Debug.Assert(_containerLock.CurrentCount == 0);
            Debug.Assert(container.ReferenceCount == 0);
        
            container.CancellationTokenSource.Cancel();

            try {
                container.ReferenceWaitTask!.Wait();
            } catch (AggregateException ae) {
                foreach (var e in ae.InnerExceptions) {
                    if (e is TaskCanceledException or OperationCanceledException) continue;
                
                    // TODO: Report
                }
            }

            container.CancellationTokenSource.Dispose();

            switch (container.ReferenceWaitTask!.Status) {
                case TaskStatus.RanToCompletion:
                    object releasedResource = container.ReferenceWaitTask.Result!;
        
                    _environment.Statistics.DecrementUniqueResourceCount();

                    bool removal = _importedObjectMap.TryRemove(releasedResource, out _);
                    Debug.Assert(removal);
        
                    removal = _resourceContainers.Remove(container.ResourceId);
                    Debug.Assert(removal);

                    if (_environment.Disposers.TryDispose(releasedResource)) {
                        _environment.Statistics.IncrementDisposedResourceCount();
                        return ReleaseStatus.Success;
                    }

                    _environment.Statistics.IncrementUndisposedResourceCount();
                    return ReleaseStatus.NotDisposed;
            
                case TaskStatus.Canceled or TaskStatus.Faulted:
                    Debug.Assert(!_resourceContainers.ContainsKey(container.ResourceId));
                    return ReleaseStatus.Canceled;
            
                default: throw new UnreachableException($"Unexpected task status '{container.ReferenceWaitTask.Status}'.");
            }
        } finally {
            _containerLock.Release();
        }
    }

    private uint DecrementResourceContainerReference(ref uint referenceCount) {
        _environment.Statistics.Release();

        return --referenceCount;
    }

    private void DisposeResources() {
        // TODO: Implement this.
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            _containerLock.Wait();

            try {
                DisposeResources();
            } finally {
                _containerLock.Release();
            }

            _containerLock.Dispose();
        }
    }
    
    public async ValueTask DisposeAsync() {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        
        await _containerLock.WaitAsync();

        try {
            DisposeResources();
        } finally {
            _containerLock.Release();
        }

        _containerLock.Dispose();
        
        GC.SuppressFinalize(this);
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
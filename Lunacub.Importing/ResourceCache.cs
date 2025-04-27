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

            return ReleaseResourceContainer(container);
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

            return ReleaseResourceContainer(container);
        } finally {
            _containerLock.Release();
        }
    }

    public ReleaseStatus Release(ResourceID rid) {
        throw new NotImplementedException();
    }

    private uint DecrementResourceContainerReference(ref uint referenceCount) {
        _environment.Statistics.DecrementTotalReferenceCount();
        _environment.Statistics.IncrementTotalDisposeCount();

        return --referenceCount;
    }

    private ReleaseStatus ReleaseResourceContainer(ResourceContainer container) {
        Debug.Assert(_containerLock.CurrentCount == 0);
        Debug.Assert(container.ReferenceCount == 0);
        
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

    private class ResourceContainer {
        public readonly ResourceID Rid;
        public Task<object?> FullImportTask;
        public Task<ResourceVessel> VesselImportTask;
        public uint ReferenceCount;

        public ResourceContainer(ResourceID rid, uint initialReferenceCount) {
            Rid = rid;
            FullImportTask = Task.FromResult<object?>(null);
            VesselImportTask = Task.FromResult<ResourceVessel>(default);
            ReferenceCount = initialReferenceCount;
        }
    }

    private readonly record struct ResourceVessel(Deserializer Deserializer, object Deserialized, DeserializationContext Context);
}
namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceCache : IDisposable {
    private readonly SemaphoreSlim _containerLock;
    private readonly Dictionary<ResourceID, ResourceContainer> _resourceContainers;
    private readonly ImportEnvironment _environment;
    
    private bool _disposed;

    internal ResourceCache(ImportEnvironment environment) {
        _containerLock =  new(1, 1);
        _resourceContainers = [];
        _environment = environment;
    }

    public ImportingOperation<T> ImportAsync<T>(ResourceID rid) where T : class {
        return new(rid, ImportSingleResource<T>(rid));
    }
    
    // public async Task<ResourceHandle<T>> ImportAsync<T>(ResourceID rid) where T : class {
    //     T? output = await ImportSingleResource(rid, typeof(T)) as T;
    //     return new(rid, output);
    // }

    public ReleaseStatus Release(object? resource) {
        throw new NotImplementedException();
    }
    
    public ReleaseStatus Release(ResourceID rid) {
        throw new NotImplementedException();
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
namespace Caxivitual.Lunacub.Importing;

public sealed class ImportEnvironment : IDisposable {
    public InputSystem Input { get; }
    public DeserializerDictionary Deserializers { get; }
    public DisposerCollection Disposers { get; }
    
    private readonly ResourceRegistry _resources;
    
    private bool _disposed;
    
    public ImportEnvironment() {
        Input = new();
        Deserializers = [];
        Disposers = [];
        _resources = new(this);
    }

    public ResourceHandle Import(ResourceID rid) => Import<object>(rid);

    public ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
        return _resources.Import<T>(rid);
    }

    public void ImportFromTags(string filter, ICollection<ResourceHandle> outputList) {
        _resources.ImportFromTags(filter, outputList);
    }

    public ReleaseStatus Release(ResourceHandle handle) {
        return _resources.Release(handle);
    }

    public ReleaseStatus Release(ResourceID rid) {
        return _resources.Release(rid);
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            _resources.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ImportEnvironment() {
        Dispose(false);
    }
}
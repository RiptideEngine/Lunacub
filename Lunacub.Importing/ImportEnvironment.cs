namespace Caxivitual.Lunacub.Importing;

public sealed class ImportEnvironment : IDisposable {
    public DeserializerDictionary Deserializers { get; }
    public InputSystem Input { get; }

    private readonly ResourceRegistry _resources;
    
    private bool _disposed;
    
    public ImportEnvironment() {
        Deserializers = [];
        Input = new();
        _resources = new(this);
    }

    public T? Import<T>(ResourceID rid) where T : class {
        return _resources.Import<T>(rid);
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
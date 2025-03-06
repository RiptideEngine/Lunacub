namespace Caxivitual.Lunacub.Importing;

public sealed class ImportingContext : IDisposable {
    public DeserializerDictionary Deserializers { get; }
    public ResourceRegistry Resources { get; }

    private bool _disposed;

    public ImportingContext() {
        Deserializers = [];
        Resources = new(this);
    }

    public T? Import<T>(ResourceID rid) where T : class {
        return Resources.Import<T>(rid);
    }

    public T? Import<T>(string path) where T : class {
        return Resources.Import<T>(path);
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            Resources.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ImportingContext() {
        Dispose(false);
    }
}
namespace Caxivitual.Lunacub.Importing;

public sealed class ResourceRegistry : IDisposable {
    public ResourceLibraryCollection Libraries { get; } = [];
    private readonly ResourceCache _cache;

    private bool _disposed;

    internal ResourceRegistry(ImportingContext context) {
        _cache = new(context);
    }
    
    internal T? Import<T>(ResourceID rid) where T : class {
        foreach (var library in Libraries) {
            if (library.TryGetValue(rid, out string? path)) {
                return (T)_cache.Import(rid, path, typeof(T));
            }
        }

        return null;
    }

    internal T? Import<T>(string path) where T : class {
        return Libraries.PathToIDMap.TryGetValue(path, out ResourceID rid) ? (T)_cache.Import(rid, path, typeof(T)) : null;
    }
    
    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            _cache.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceRegistry() {
        Dispose(false);
    }
}
namespace Caxivitual.Lunacub.Building;

public sealed class ResourceRegistry : IEnumerable<KeyValuePair<ResourceID, ResourceRegistry.BuildingResource>>, IDisposable {
    private readonly Dictionary<ResourceID, BuildingResource> _dict;

    public int Count => _dict.Count;

    private readonly ReaderWriterLockSlim _lock;
    private bool _disposed;

    internal ResourceRegistry() {
        _dict = [];
        _lock = new();
    }
    
    public void Add(ResourceID id, string path, in BuildingOptions options) {
        if (_dict.ContainsKey(id)) throw new ArgumentException($"Resource ID '{id}' has already been registered.");
        ValidateOptions(options);

        string fullPath = Path.GetFullPath(path);

        _lock.EnterWriteLock();

        try {
            if (_dict.ContainsKey(id)) {
                throw new ArgumentException($"ResourceID '{id}' already registered.");
            }
            
            _dict.Add(id, new(fullPath, options));
        } finally {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(ResourceID id) => Remove(id, out _);
    
    public bool Remove(ResourceID id, out BuildingResource output) {
        _lock.EnterWriteLock();
        try {
            if (_dict.Remove(id, out output)) {
                return true;
            }
        } finally {
            _lock.ExitWriteLock();
        }

        output = default;
        return false;
    }

    public bool Contains(ResourceID id) {
        _lock.EnterReadLock();
        try {
            return _dict.ContainsKey(id);
        } finally {
            _lock.ExitReadLock();
        }
    }

    public void Clear() {
        _lock.EnterWriteLock();
        try {
            _dict.Clear();
        } finally {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGet(ResourceID id, out BuildingResource output) {
        _lock.EnterReadLock();
        try {
            return _dict.TryGetValue(id, out output);
        } finally {
            _lock.ExitReadLock();
        }
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;
        
        _lock.Dispose();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<KeyValuePair<ResourceID, BuildingResource>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    
    [ExcludeFromCodeCoverage]
    ~ResourceRegistry() {
        Dispose(false);
    }

    [StackTraceHidden]
    private static void ValidateOptions(BuildingOptions options) {
        if (string.IsNullOrWhiteSpace(options.ImporterName)) {
            throw new ArgumentException("ImporterName cannot be null, empty or consist of only whitespace characters.");
        }
    }

    public readonly record struct BuildingResource(string Path, BuildingOptions Options);
}
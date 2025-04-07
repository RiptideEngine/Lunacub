namespace Caxivitual.Lunacub.Building;

public sealed class ResourceRegistry : IEnumerable<KeyValuePair<ResourceID, BuildingOptions>>, IDisposable {
    private readonly Dictionary<string, ResourceID> _pathToRidMap;
    private readonly Dictionary<ResourceID, string> _ridToPathMap;
    private readonly Dictionary<ResourceID, BuildingOptions> _buildOptions;

    public int Count => _buildOptions.Count;

    private readonly ReaderWriterLockSlim _lock;
    private bool _disposed;

    internal ResourceRegistry() {
        _buildOptions = [];
        _ridToPathMap = [];
        _pathToRidMap = [];

        _lock = new();
    }
    
    public void Add(ResourceID id, string path, in BuildingOptions options) {
        if (_buildOptions.ContainsKey(id)) throw new ArgumentException($"Resource ID '{id}' has already been registered.");
        ValidateOptions(options);

        string fullPath = Path.GetFullPath(path);

        _lock.EnterWriteLock();

        try {
            if (_pathToRidMap.ContainsKey(fullPath)) {
                throw new ArgumentException($"Resource path '{fullPath}' already registered.");
            }
            
            if (_ridToPathMap.ContainsKey(id)) {
                throw new ArgumentException($"ResourceID '{id}' already registered.");
            }
            
            _buildOptions.Add(id, options);
            _ridToPathMap.Add(id, fullPath);
            _pathToRidMap.Add(fullPath, id);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(ResourceID id) => Remove(id, out _, out _);
    
    public bool Remove(ResourceID id, [NotNullWhen(true)] out string? path, out BuildingOptions options) {
        _lock.EnterWriteLock();
        try {
            if (_ridToPathMap.Remove(id, out path)) {
                _pathToRidMap.Remove(path);
                _buildOptions.Remove(id, out options);

                return true;
            }
        } finally {
            _lock.ExitWriteLock();
        }

        path = null;
        options = default;
        return false;
    }
    
    public bool Remove(string path) => Remove(path, out _, out _);

    public bool Remove(string path, out ResourceID id, out BuildingOptions options) {
        var fullPath = Path.GetFullPath(path);

        _lock.EnterWriteLock();

        try {
            if (_pathToRidMap.Remove(fullPath, out id)) {
                _ridToPathMap.Remove(id);
                _buildOptions.Remove(id, out options);

                return true;
            }
        } finally {
            _lock.ExitWriteLock();
        }

        id = ResourceID.Null;
        options = default;
        return false;
    }

    public bool Contains(ResourceID id) {
        _lock.EnterReadLock();
        try {
            return _buildOptions.ContainsKey(id);
        } finally {
            _lock.ExitReadLock();
        }
    }
    
    public bool Contains(string path) {
        _lock.EnterReadLock();
        try {
            return _pathToRidMap.ContainsKey(Path.GetFullPath(path));
        } finally {
            _lock.ExitReadLock();
        }
    }

    public void Clear() {
        _lock.EnterWriteLock();
        try {
            _buildOptions.Clear();
            _pathToRidMap.Clear();
            _ridToPathMap.Clear();
        } finally {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGet(ResourceID id, [NotNullWhen(true)] out string? path, out BuildingOptions options) {
        _lock.EnterReadLock();
        try {
            if (_ridToPathMap.TryGetValue(id, out path)) {
                options = _buildOptions[id];
                return true;
            }
        } finally {
            _lock.ExitReadLock();
        }

        path = null;
        options = default;
        return false;
    }
    
    public bool TryGet(string path, out ResourceID id, out BuildingOptions options) {
        string fullPath = Path.GetFullPath(path);
        
        _lock.EnterReadLock();
        try {
            if (_pathToRidMap.TryGetValue(fullPath, out id)) {
                options = _buildOptions[id];
                return true;
            }
        } finally {
            _lock.ExitReadLock();
        }

        id = ResourceID.Null;
        options = default;
        return false;
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

    public IEnumerator<KeyValuePair<ResourceID, BuildingOptions>> GetEnumerator() => _buildOptions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_buildOptions).GetEnumerator();
    
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
}
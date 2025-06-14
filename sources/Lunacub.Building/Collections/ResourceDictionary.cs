namespace Caxivitual.Lunacub.Building.Collections;

/// <summary>
/// Represents a collection that contains all the resources need to be built.
/// </summary>
public sealed class ResourceDictionary : IDictionary<ResourceID, BuildingResource>, IDisposable {
    private readonly ReaderWriterLockSlim _lock;
    private readonly Dictionary<ResourceID, BuildingResource> _dict;

    public int Count => _dict.Count;
    
    [ExcludeFromCodeCoverage] bool ICollection<KeyValuePair<ResourceID, BuildingResource>>.IsReadOnly => false;
    
    [ExcludeFromCodeCoverage] ICollection<ResourceID> IDictionary<ResourceID, BuildingResource>.Keys => _dict.Keys;
    [ExcludeFromCodeCoverage] ICollection<BuildingResource> IDictionary<ResourceID, BuildingResource>.Values => _dict.Values;

    private bool _disposed;

    internal ResourceDictionary() {
        _dict = [];
        _lock = new();
    }
    
    public void Add(ResourceID id, BuildingResource resource) {
        if (id == ResourceID.Null) throw new ArgumentException("Resource ID cannot be 0.");

        ValidateResource(resource);

        _lock.EnterWriteLock();
        try {
            _dict.Add(id, resource);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    void ICollection<KeyValuePair<ResourceID, BuildingResource>>.Add(KeyValuePair<ResourceID, BuildingResource> kvp) {
        _lock.EnterWriteLock();
        try {
            if (kvp.Key == ResourceID.Null) throw new ArgumentException("Resource ID cannot be 0.");
            
            ValidateResource(kvp.Value);
            
            ((IDictionary<ResourceID, BuildingResource>)_dict).Add(kvp);
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

    bool ICollection<KeyValuePair<ResourceID, BuildingResource>>.Remove(KeyValuePair<ResourceID, BuildingResource> kvp) {
        _lock.EnterWriteLock();
        try {
            return ((IDictionary<ResourceID, BuildingResource>)_dict).Remove(kvp);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    public bool ContainsKey(ResourceID id) {
        _lock.EnterReadLock();
        try {
            return _dict.ContainsKey(id);
        } finally {
            _lock.ExitReadLock();
        }
    }
    
    bool ICollection<KeyValuePair<ResourceID, BuildingResource>>.Contains(KeyValuePair<ResourceID, BuildingResource> kvp) {
        _lock.EnterReadLock();
        try {
            return ((IDictionary<ResourceID, BuildingResource>)_dict).Contains(kvp);
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

    public bool TryGetValue(ResourceID id, out BuildingResource output) {
        _lock.EnterReadLock();
        try {
            return _dict.TryGetValue(id, out output);
        } finally {
            _lock.ExitReadLock();
        }
    }

    public BuildingResource this[ResourceID key] {
        get {
            _lock.EnterReadLock();
            try {
                return _dict[key];
            } finally {
                _lock.ExitReadLock();
            }
        }

        set {
            ValidateResource(value);
            
            _lock.EnterWriteLock();
            try {
                _dict[key] = value;
            } finally {
                _lock.ExitWriteLock();
            }
        }
    }

    void ICollection<KeyValuePair<ResourceID, BuildingResource>>.CopyTo(KeyValuePair<ResourceID, BuildingResource>[] array, int arrayIndex) {
        _lock.EnterReadLock();
        try {
            ((ICollection<KeyValuePair<ResourceID, BuildingResource>>)_dict).CopyTo(array, arrayIndex);
        } finally {
            _lock.ExitReadLock();
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        _lock.Dispose();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Dictionary<ResourceID, BuildingResource>.Enumerator GetEnumerator() => _dict.GetEnumerator();
    
    IEnumerator<KeyValuePair<ResourceID, BuildingResource>> IEnumerable<KeyValuePair<ResourceID, BuildingResource>>.GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    
    [ExcludeFromCodeCoverage]
    ~ResourceDictionary() {
        Dispose(false);
    }

    [StackTraceHidden]
    private static void ValidateResource(in BuildingResource resource) {
        if (resource.Provider == null) throw new ArgumentException("Resource provider is null.");
        
        if (string.IsNullOrWhiteSpace(resource.Options.ImporterName)) {
            throw new ArgumentException("ImporterName cannot be null, empty or consist of only whitespace characters.");
        }
    }
}
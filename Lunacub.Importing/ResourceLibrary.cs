using System.Collections;

namespace Caxivitual.Lunacub.Importing;

public sealed class ResourceLibrary(Guid id) : IEnumerable<KeyValuePair<ResourceID, string>> {
    public Guid Id { get; } = id;
    private readonly Dictionary<ResourceID, string> _resources = [];
    
    public int Count => _resources.Count;

    public void Add(ResourceID rid, string path) {
        if (rid == default) {
            throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
        }
        
        _resources.Add(rid, path);
    }

    public bool TryAdd(ResourceID rid, string path) {
        if (rid == default) {
            throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
        }
        
        return _resources.TryAdd(rid, path);
    }

    public bool Remove(ResourceID rid) => _resources.Remove(rid);
    
    public bool TryGetValue(ResourceID rid, [NotNullWhen(true)] out string? path) => _resources.Remove(rid, out path);
    
    public bool Contains(ResourceID rid) => _resources.ContainsKey(rid);
    
    public void Clear() => _resources.Clear();
    
    public bool TryGet(ResourceID rid, [NotNullWhen(true)] out string? path) => _resources.TryGetValue(rid, out path);

    public string this[ResourceID rid] {
        get => _resources[rid];
        set {
            if (rid == default) {
                throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
            }
            
            _resources[rid] = value;
        }
    }
    
    public Dictionary<ResourceID, string>.Enumerator GetEnumerator() => _resources.GetEnumerator();
    
    IEnumerator<KeyValuePair<ResourceID, string>> IEnumerable<KeyValuePair<ResourceID, string>>.GetEnumerator() => _resources.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
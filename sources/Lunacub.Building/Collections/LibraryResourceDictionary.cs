namespace Caxivitual.Lunacub.Building.Collections;

public class LibraryResourceDictionary<T> : IDictionary<ResourceID, T>, IReadOnlyDictionary<ResourceID, T> {
    protected readonly Dictionary<ResourceID, T> _dict = [];
    
    public int Count => _dict.Count;

    bool ICollection<KeyValuePair<ResourceID, T>>.IsReadOnly => false;
    
    ICollection<ResourceID> IDictionary<ResourceID, T>.Keys => _dict.Keys;
    ICollection<T> IDictionary<ResourceID, T>.Values => _dict.Values;
    
    IEnumerable<ResourceID> IReadOnlyDictionary<ResourceID, T>.Keys => _dict.Keys;
    IEnumerable<T> IReadOnlyDictionary<ResourceID, T>.Values => _dict.Values;

    public void Add(ResourceID key, T value) => _dict.Add(key, value);

    void ICollection<KeyValuePair<ResourceID, T>>.Add(KeyValuePair<ResourceID, T> item) {
        ((ICollection<KeyValuePair<ResourceID, T>>)_dict).Add(item);
    }
    
    public bool Remove(ResourceID key) => _dict.Remove(key);

    public bool Remove(ResourceID key, [NotNullWhen(true)] out T? value) => _dict.Remove(key, out value);

    bool ICollection<KeyValuePair<ResourceID, T>>.Remove(KeyValuePair<ResourceID, T> item) {
        return ((ICollection<KeyValuePair<ResourceID, T>>)_dict).Remove(item);
    }
    
    public void Clear() => _dict.Clear();
    
    public bool ContainsKey(ResourceID key) => _dict.ContainsKey(key);

    bool ICollection<KeyValuePair<ResourceID, T>>.Contains(KeyValuePair<ResourceID, T> item) {
        return ((ICollection<KeyValuePair<ResourceID, T>>)_dict).Contains(item);
    }
    
    public bool TryGetValue(ResourceID key, [NotNullWhen(true)] out T? value) {
        return _dict.TryGetValue(key, out value);
    }

    public T this[ResourceID key] {
        get => _dict[key];
        set => _dict[key] = value;
    }

    void ICollection<KeyValuePair<ResourceID, T>>.CopyTo(KeyValuePair<ResourceID, T>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<ResourceID, T>>)_dict).CopyTo(array, arrayIndex);
    }
    
    public Dictionary<ResourceID, T>.Enumerator GetEnumerator() => _dict.GetEnumerator();

    IEnumerator<KeyValuePair<ResourceID, T>> IEnumerable<KeyValuePair<ResourceID, T>>.GetEnumerator() {
        return ((IEnumerable<KeyValuePair<ResourceID, T>>)_dict).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
}
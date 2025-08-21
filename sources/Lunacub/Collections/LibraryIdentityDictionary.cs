namespace Caxivitual.Lunacub.Collections;

public class LibraryIdentityDictionary<T> : IDictionary<LibraryID, T>, IReadOnlyDictionary<LibraryID, T> {
    protected readonly Dictionary<LibraryID, T> _dict = [];
    
    public int Count => _dict.Count;

    bool ICollection<KeyValuePair<LibraryID, T>>.IsReadOnly => false;
    
    ICollection<LibraryID> IDictionary<LibraryID, T>.Keys => _dict.Keys;
    ICollection<T> IDictionary<LibraryID, T>.Values => _dict.Values;
    
    IEnumerable<LibraryID> IReadOnlyDictionary<LibraryID, T>.Keys => _dict.Keys;
    IEnumerable<T> IReadOnlyDictionary<LibraryID, T>.Values => _dict.Values;

    public void Add(LibraryID key, T value) => _dict.Add(key, value);

    void ICollection<KeyValuePair<LibraryID, T>>.Add(KeyValuePair<LibraryID, T> item) {
        ((ICollection<KeyValuePair<LibraryID, T>>)_dict).Add(item);
    }
    
    public bool Remove(LibraryID key) => _dict.Remove(key);

    bool ICollection<KeyValuePair<LibraryID, T>>.Remove(KeyValuePair<LibraryID, T> item) {
        return ((ICollection<KeyValuePair<LibraryID, T>>)_dict).Remove(item);
    }
    
    public void Clear() => _dict.Clear();
    
    public bool ContainsKey(LibraryID key) => _dict.ContainsKey(key);

    bool ICollection<KeyValuePair<LibraryID, T>>.Contains(KeyValuePair<LibraryID, T> item) {
        return ((ICollection<KeyValuePair<LibraryID, T>>)_dict).Contains(item);
    }
    
    public bool TryGetValue(LibraryID key, [NotNullWhen(true)] out T? value) {
        return _dict.TryGetValue(key, out value);
    }

    public T this[LibraryID key] {
        get => _dict[key];
        set => _dict[key] = value;
    }

    void ICollection<KeyValuePair<LibraryID, T>>.CopyTo(KeyValuePair<LibraryID, T>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<LibraryID, T>>)_dict).CopyTo(array, arrayIndex);
    }
    
    public Dictionary<LibraryID, T>.Enumerator GetEnumerator() => _dict.GetEnumerator();

    IEnumerator<KeyValuePair<LibraryID, T>> IEnumerable<KeyValuePair<LibraryID, T>>.GetEnumerator() {
        return ((IEnumerable<KeyValuePair<LibraryID, T>>)_dict).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
}
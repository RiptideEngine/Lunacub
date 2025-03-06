using System.Collections;

namespace Caxivitual.Lunacub;

public abstract class IdentityDictionary<T>(IEqualityComparer<string> comparer) : IDictionary<string, T> {
    protected readonly Dictionary<string, T> _dict = new(comparer);
    
    public int Count => _dict.Count;
    
    ICollection<string> IDictionary<string, T>.Keys => ((IDictionary<string, T>)_dict).Keys;
    ICollection<T> IDictionary<string, T>.Values => ((IDictionary<string, T>)_dict).Values;

    bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

    public abstract void Add(string key, T value);
    
    public abstract bool TryAdd(string key, T value);
    public abstract bool TryAdd(ReadOnlySpan<char> key, T value);

    void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item) {
        Add(item.Key, item.Value);
    }

    public abstract bool Remove(string key);
    public abstract bool Remove(string key, [NotNullWhen(true)] out T? value);
    
    public abstract bool Remove(ReadOnlySpan<char> key);
    public abstract bool Remove(ReadOnlySpan<char> key, [NotNullWhen(true)] out T? value);
    
    bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> kvp) {
        if (TryGetValue(kvp.Key, out T? value) && EqualityComparer<T>.Default.Equals(value, kvp.Value)) {
            Remove(kvp.Key);
            return true;
        }

        return false;
    }
    
    public void Clear() {
        _dict.Clear();
    }

    public abstract bool ContainsKey(string key);
    public abstract bool ContainsKey(ReadOnlySpan<char> key);
    
    bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> kvp) {
        return TryGetValue(kvp.Key, out T? value) && EqualityComparer<T>.Default.Equals(value, kvp.Value);
    }
    
    public abstract bool TryGetValue(string key, [MaybeNullWhen(false)] out T value);
    public abstract bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out T value);
    
    public abstract T this[string key] { get; set; }
    public abstract T this[ReadOnlySpan<char> key] { get; set; }
    
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

    void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<string, T>>)_dict).CopyTo(array, arrayIndex);
    }
}
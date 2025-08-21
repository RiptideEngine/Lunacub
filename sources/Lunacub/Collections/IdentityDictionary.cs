namespace Caxivitual.Lunacub.Collections;

/// <summary>
/// Represents a <see cref="Dictionary{TKey, TValue}"/> that uses <see cref="string"/> as key.
/// </summary>
/// <typeparam name="T">The type of the values in the dictionary.</typeparam>
[ExcludeFromCodeCoverage]
public class IdentityDictionary<T> : IDictionary<string, T> {
    protected readonly Dictionary<string, T> _dict;
    
    public int Count => _dict.Count;
    
    ICollection<string> IDictionary<string, T>.Keys => ((IDictionary<string, T>)_dict).Keys;
    ICollection<T> IDictionary<string, T>.Values => ((IDictionary<string, T>)_dict).Values;

    bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;
    
    public IdentityDictionary(IEqualityComparer<string> comparer) {
        if (comparer is not IAlternateEqualityComparer<ReadOnlySpan<char>, string?>) {
            string message = string.Format(
                ExceptionMessages.RequiresComparerImplementsAlternateLookup, 
                "ReadOnlySpan<char>", 
                "string"
            );
            
            throw new ArgumentException(message, nameof(comparer));
        }
            
        _dict = new(comparer);
    }

    protected virtual void Validate(ReadOnlySpan<char> key, T value) {
        if (key.IsEmpty) {
            throw new ArgumentException(ExceptionMessages.EmptyOrWhitespaceKey, nameof(key));
        }
    }
    
    protected virtual bool TryValidate(ReadOnlySpan<char> key, T value) {
        if (key.IsEmpty) return false;

        return true;
    }

    public void Add(string key, T value) {
        Validate(key, value);
        
        _dict.Add(key, value);
    }

    public bool TryAdd([NotNullWhen(true)] string? key, T value) {
        if (key == null || !TryValidate(key, value)) return false;

        ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, key, out bool exists);
        if (exists) return false;

        reference = value;
        return true;
    }

    public bool TryAdd(ReadOnlySpan<char> key, T value) {
        if (key.IsEmpty || !TryValidate(key, value)) return false;

        ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict.GetAlternateLookup<ReadOnlySpan<char>>(), key, out bool exists);
        if (exists) return false;
        
        reference = value;
        return true;
    }

    void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item) {
        Add(item.Key, item.Value);
    }

    public bool Remove(string key) => _dict.Remove(key);
    public bool Remove(string key, [NotNullWhen(true)] out T? output) => _dict.Remove(key, out output);

    public bool Remove(ReadOnlySpan<char> key) => _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key);
    public bool Remove(ReadOnlySpan<char> key, [NotNullWhen(true)] out string? actualKey, [NotNullWhen(true)] out T? output) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key, out actualKey, out output);
    }
    
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

    public bool ContainsKey(string key) => _dict.ContainsKey(key);
    public bool ContainsKey(ReadOnlySpan<char> key) => _dict.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(key);
    
    bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> kvp) {
        return TryGetValue(kvp.Key, out T? value) && EqualityComparer<T>.Default.Equals(value, kvp.Value);
    }
    
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T output) => _dict.TryGetValue(key, out output);
    public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out T output) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(key, out output);
    }

    public T this[string key] {
        get => _dict[key];
        set {
            Validate(key, value);
            
            _dict[key] = value;
        }
    }

    public T this[ReadOnlySpan<char> key] {
        get => _dict.GetAlternateLookup<ReadOnlySpan<char>>()[key];
        set {
            Validate(key, value);
            
            var lookup = _dict.GetAlternateLookup<ReadOnlySpan<char>>();
            lookup[key] = value;
        }
    }
    
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

    void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<string, T>>)_dict).CopyTo(array, arrayIndex);
    }
}
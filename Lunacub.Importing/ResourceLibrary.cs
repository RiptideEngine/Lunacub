namespace Caxivitual.Lunacub.Importing;

public abstract class ResourceLibrary {
    public Guid Id { get; }

    protected ResourceLibrary(Guid id) {
        Id = id;
    }

    public abstract bool Contains(ResourceID rid);
    public abstract Stream CreateStream(ResourceID rid);

    // public Guid Id { get; } = id;
    // private readonly Dictionary<ResourceID, string> _resources = [];
    //
    // public int Count => _resources.Count;
    //
    // public void Add(ResourceID rid, string path) {
    //     if (rid == default) {
    //         throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
    //     }
    //     
    //     _resources.Add(rid, path);
    // }
    //
    // void ICollection<KeyValuePair<ResourceID, string>>.Add(KeyValuePair<ResourceID, string> item) {
    //     if (item.Key == default) {
    //         throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
    //     }
    //     
    //     ((ICollection<KeyValuePair<ResourceID, string>>)_resources).Add(item);
    // }
    //
    // public bool TryAdd(ResourceID rid, string path) {
    //     if (rid == default) {
    //         throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
    //     }
    //     
    //     return _resources.TryAdd(rid, path);
    // }
    //
    // public bool Remove(ResourceID rid) => _resources.Remove(rid);
    //
    // bool ICollection<KeyValuePair<ResourceID, string>>.Remove(KeyValuePair<ResourceID, string> item) {
    //     return ((ICollection<KeyValuePair<ResourceID, string>>)_resources).Remove(item);
    // }
    //
    // public bool TryGetValue(ResourceID rid, [NotNullWhen(true)] out string? path) => _resources.Remove(rid, out path);
    //
    // public bool ContainsKey(ResourceID rid) => _resources.ContainsKey(rid);
    //
    // bool ICollection<KeyValuePair<ResourceID, string>>.Contains(KeyValuePair<ResourceID, string> item) {
    //     return ((ICollection<KeyValuePair<ResourceID, string>>)_resources).Contains(item);
    // }
    //
    // public void Clear() => _resources.Clear();
    //
    // public bool TryGet(ResourceID rid, [NotNullWhen(true)] out string? path) => _resources.TryGetValue(rid, out path);
    //
    // public string this[ResourceID rid] {
    //     get => _resources[rid];
    //     set {
    //         if (rid == default) {
    //             throw new ArgumentException($"{nameof(ResourceID)} cannot be default value.");
    //         }
    //         
    //         _resources[rid] = value;
    //     }
    // }
    //
    // void ICollection<KeyValuePair<ResourceID, string>>.CopyTo(KeyValuePair<ResourceID, string>[] array, int arrayIndex) {
    //     ((ICollection<KeyValuePair<ResourceID, string>>)_resources).CopyTo(array, arrayIndex);
    // }
    //
    // public Dictionary<ResourceID, string>.Enumerator GetEnumerator() => _resources.GetEnumerator();
    //
    // IEnumerator<KeyValuePair<ResourceID, string>> IEnumerable<KeyValuePair<ResourceID, string>>.GetEnumerator() => _resources.GetEnumerator();
    // IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
using Caxivitual.Lunacub.Serialization;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceRegistryJsonConverterFactory))]
public class ResourceRegistry<T> : IDictionary<ResourceID, T> where T : IRegistryElement {
    private readonly Dictionary<ResourceID, T> _resources = [];
    private readonly Dictionary<string, ResourceID> _nameMap = [];
    
    public int Count => _resources.Count;

    bool ICollection<KeyValuePair<ResourceID, T>>.IsReadOnly => false;
    
    public Dictionary<ResourceID, T>.KeyCollection Keys => _resources.Keys;
    public Dictionary<ResourceID, T>.ValueCollection Values => _resources.Values;
    
    ICollection<ResourceID> IDictionary<ResourceID, T>.Keys => _resources.Keys;
    ICollection<T> IDictionary<ResourceID, T>.Values => _resources.Values;

    public void Add(ResourceID resourceId, T element) {
        ValidateResourceId(resourceId);
        ValidateElement(element);

        ref var elementReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourceId, out bool exists);

        if (exists) {
            throw new ArgumentException($"Resource Id {resourceId} is already registered.");
        }

        ref var nameMappingReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, element.Name, out exists);

        if (exists) {
            throw new ArgumentException($"Resource name '{element.Name}' is already registered to resource with id {nameMappingReference}.");
        }

        elementReference = element;
        nameMappingReference = resourceId;
    }

    void ICollection<KeyValuePair<ResourceID, T>>.Add(KeyValuePair<ResourceID, T> item) {
        Add(item.Key, item.Value);
    }

    public bool Remove(ResourceID resourceId) {
        if (_resources.Remove(resourceId, out var element)) {
            bool removal = _nameMap.Remove(element.Name);
            Debug.Assert(removal);

            return true;
        }

        return false;
    }
    
    public bool Remove(ResourceID resourceId, [NotNullWhen(true)] out T? output) {
        if (_resources.Remove(resourceId, out output)) {
            bool removal = _nameMap.Remove(output.Name);
            Debug.Assert(removal);

            return true;
        }

        return false;
    }

    public bool Remove(string name) {
        if (_nameMap.Remove(name, out var resourceId)) {
            bool removal = _resources.Remove(resourceId);
            Debug.Assert(removal);

            return true;
        }

        return false;
    }

    public bool Remove(string name, [NotNullWhen(true)] out T? output) {
        if (_nameMap.Remove(name, out var resourceId)) {
            bool removal = _resources.Remove(resourceId, out output);
            Debug.Assert(removal);

#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }

        output = default;
        return false;
    }

    bool ICollection<KeyValuePair<ResourceID, T>>.Remove(KeyValuePair<ResourceID, T> item) {
        if (((ICollection<KeyValuePair<ResourceID, T>>)_resources).Remove(item)) {
            bool removal = _nameMap.Remove(item.Value.Name);
            Debug.Assert(removal);

            return true;
        }

        return false;
    }
    
    public void Clear() {
        _resources.Clear();
        _nameMap.Clear();
    }
    
    public bool ContainsKey(ResourceID resourceId) => _resources.ContainsKey(resourceId);
    
    bool ICollection<KeyValuePair<ResourceID, T>>.Contains(KeyValuePair<ResourceID, T> item) => _resources.Contains(item);
    
    public bool TryGetValue(ResourceID resourceId, [NotNullWhen(true)] out T? output) => _resources.TryGetValue(resourceId, out output);

    public T this[ResourceID resourceId] {
        get => _resources[resourceId];
        set {
            ValidateResourceId(resourceId);
            ValidateElement(value);
            
            ref var nameMapReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, value.Name, out bool exists);
            
            if (exists) {
                throw new ArgumentException($"Resource name '{value.Name}' is already registered to resource with id {nameMapReference}.");
            }
            
            ref var elementReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourceId, out exists);

            if (exists) {
                bool removal = _nameMap.Remove(value.Name);
                Debug.Assert(removal);
            }

            elementReference = value;
            nameMapReference = resourceId;
        }
    }

    void ICollection<KeyValuePair<ResourceID, T>>.CopyTo(KeyValuePair<ResourceID, T>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<ResourceID, T>>)_resources).CopyTo(array, arrayIndex);
    }

    [StackTraceHidden]
    private static void ValidateResourceId(ResourceID resourceId) {
        if (resourceId == ResourceID.Null) {
            throw new ArgumentException("Resource ID cannot be null or zero value.", nameof(resourceId));
        }
    }

    [StackTraceHidden]
    protected virtual void ValidateElement(T? element) {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        
        if (string.IsNullOrEmpty(element.Name)) {
            throw new ArgumentException("Resource name cannot be null or empty string.", nameof(element));
        }

        for (int i = 0, e = element.Tags.Length; i < e; i++) {
            var tag = element.Tags[i];

            if (string.IsNullOrEmpty(tag)) {
                throw new ArgumentException($"Tag index {i} cannot be null or empty string.", nameof(element));
            }

            int invalidCharacterIndex = tag.AsSpan().IndexOfAnyExcept(Constants.ValidTagCharacters);
            if (invalidCharacterIndex != -1) {
                throw new ArgumentException($"Tag '{tag}' contains invalid character at position {invalidCharacterIndex}.", nameof(element));
            }
        }
    }

    public Dictionary<ResourceID, T>.Enumerator GetEnumerator() => _resources.GetEnumerator();

    IEnumerator<KeyValuePair<ResourceID, T>> IEnumerable<KeyValuePair<ResourceID, T>>.GetEnumerator() => _resources.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    // public readonly record struct T(string Name, ImmutableArray<string> Tags, TOption Option);
}

file static class Constants {
    public static System.Buffers.SearchValues<char> ValidTagCharacters { get; } = System.Buffers.SearchValues.Create("_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}
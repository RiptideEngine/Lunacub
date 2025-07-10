using Caxivitual.Lunacub.Serialization;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceRegistryJsonConverterFactory))]
public class ResourceRegistry<TElement> : IDictionary<ResourceID, TElement> where TElement : IResourceRegistryElement {
    private readonly Dictionary<ResourceID, TElement> _resources = [];
    private readonly Dictionary<string, ResourceID> _nameMap = [];
    
    public int Count => _resources.Count;

    bool ICollection<KeyValuePair<ResourceID, TElement>>.IsReadOnly => false;
    
    public Dictionary<ResourceID, TElement>.KeyCollection Keys => _resources.Keys;
    public Dictionary<ResourceID, TElement>.ValueCollection Values => _resources.Values;
    
    ICollection<ResourceID> IDictionary<ResourceID, TElement>.Keys => _resources.Keys;
    ICollection<TElement> IDictionary<ResourceID, TElement>.Values => _resources.Values;
    
    // TODO: Probably adding Notifications.

    public void Add(ResourceID resourceId, TElement element) {
        ValidateResourceId(resourceId);
        ValidateElement(element);

        if (_resources.ContainsKey(resourceId)) {
            string message = string.Format(ExceptionMessages.ResourceIdAlreadyRegistered, resourceId.ToString());
            throw new ArgumentException(message, nameof(resourceId));
        }
        
        if (_nameMap.TryGetValue(element.Name, out ResourceID nameId)) {
            string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, element.Name, nameId.ToString());
            throw new ArgumentException(message, nameof(element));
        }
        
        _resources.Add(resourceId, element);
        _nameMap.Add(element.Name, resourceId);
    }

    void ICollection<KeyValuePair<ResourceID, TElement>>.Add(KeyValuePair<ResourceID, TElement> item) {
        ValidateResourceId(item.Key);
        ValidateElement(item.Value);
        
        Add(item.Key, item.Value);
    }

    public bool Remove(ResourceID resourceId) => Remove(resourceId, out _);
    
    public bool Remove(ResourceID resourceId, [NotNullWhen(true)] out TElement? output) {
        if (_resources.Remove(resourceId, out output)) {
            bool removal = _nameMap.Remove(output.Name);
            Debug.Assert(removal);

            return true;
        }

        return false;
    }

    public bool Remove(string name) => Remove(name, out _);

    public bool Remove(string name, [NotNullWhen(true)] out TElement? output) {
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

    bool ICollection<KeyValuePair<ResourceID, TElement>>.Remove(KeyValuePair<ResourceID, TElement> item) {
        if (((ICollection<KeyValuePair<ResourceID, TElement>>)_resources).Remove(item)) {
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

    public bool ContainsKey(string name) => _nameMap.ContainsKey(name);

    bool ICollection<KeyValuePair<ResourceID, TElement>>.Contains(KeyValuePair<ResourceID, TElement> item) {
        return _resources.Contains(item);
    }

    public bool TryGetValue(ResourceID resourceId, [NotNullWhen(true)] out TElement? output) {
        return _resources.TryGetValue(resourceId, out output);
    }

    public bool TryGetValue(string name, [NotNullWhen(true)] out TElement? output) {
        if (_nameMap.TryGetValue(name, out var id)) {
            output = _resources[id];
            return true;
        }

        output = default;
        return false;
    }

    public TElement this[ResourceID resourceId] {
        get => _resources[resourceId];
        set {
            ValidateResourceId(resourceId);
            ValidateElement(value);

            if (_nameMap.TryGetValue(value.Name, out ResourceID nameId)) {
                string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, value.Name, nameId.ToString());
                throw new ArgumentException(message, nameof(value));
            }

            ref var elementReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourceId, out bool exists);
            
            if (exists) {
                bool removal = _nameMap.Remove(value.Name);
                Debug.Assert(removal);
            }
            
            elementReference = value;
            _nameMap.Add(value.Name, resourceId);
        }
    }

    void ICollection<KeyValuePair<ResourceID, TElement>>.CopyTo(KeyValuePair<ResourceID, TElement>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<ResourceID, TElement>>)_resources).CopyTo(array, arrayIndex);
    }

    [StackTraceHidden]
    private static void ValidateResourceId(ResourceID resourceId) {
        if (resourceId == ResourceID.Null) {
            throw new ArgumentException("Resource ID cannot be null or zero value.", nameof(resourceId));
        }
    }

    [StackTraceHidden]
    protected virtual void ValidateElement(TElement element) {
        ArgumentNullException.ThrowIfNull(element);
        
        if (string.IsNullOrEmpty(element.Name)) {
            throw new ArgumentException(ExceptionMessages.DisallowNullOrEmptyResourceName, nameof(element));
        }
        
        for (int i = 0, e = element.Tags.Length; i < e; i++) {
            var tag = element.Tags[i];

            if (string.IsNullOrEmpty(tag)) {
                string message = string.Format(ExceptionMessages.DisallowNullOrEmptyTag, i);
                throw new ArgumentException(message, nameof(element));
            }

            int invalidCharacterIndex = tag.AsSpan().IndexOfAnyExcept(Constants.ValidTagCharacters);
            if (invalidCharacterIndex != -1) {
                string message = string.Format(ExceptionMessages.InvalidTagCharacter, tag, invalidCharacterIndex);
                throw new ArgumentException(message, nameof(element));
            }
        }
    }

    public Dictionary<ResourceID, TElement>.Enumerator GetEnumerator() => _resources.GetEnumerator();

    IEnumerator<KeyValuePair<ResourceID, TElement>> IEnumerable<KeyValuePair<ResourceID, TElement>>.GetEnumerator() {
        return _resources.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

file static class Constants {
    public static System.Buffers.SearchValues<char> ValidTagCharacters { get; } = 
        System.Buffers.SearchValues.Create("_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}
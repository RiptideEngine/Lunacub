using Caxivitual.Lunacub.Serialization;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceRegistryJsonConverterFactory))]
public class ResourceRegistry<T> : IDictionary<ResourceID, ResourceRegistry<T>.Element> {
    private readonly Dictionary<ResourceID, Element> _resources = [];
    private readonly Dictionary<string, ResourceID> _nameMap = [];
    
    public int Count => _resources.Count;

    bool ICollection<KeyValuePair<ResourceID, Element>>.IsReadOnly => false;
    
    public Dictionary<ResourceID, Element>.KeyCollection Keys => _resources.Keys;
    public Dictionary<ResourceID, Element>.ValueCollection Values => _resources.Values;
    
    ICollection<ResourceID> IDictionary<ResourceID, Element>.Keys => _resources.Keys;
    ICollection<Element> IDictionary<ResourceID, Element>.Values => _resources.Values;
    
    // TODO: Probably adding Notifications.

    public void Add(ResourceID resourceId, Element element) {
        ValidateResourceId(resourceId);
        ValidateElement(element);

        ref var elementReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourceId, out bool exists);

        if (exists) {
            string message = string.Format(ExceptionMessages.ResourceIdAlreadyRegistered, resourceId.ToString());
            throw new ArgumentException(message, nameof(resourceId));
        }

        ref var nameMappingReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, element.Name, out exists);

        if (exists) {
            string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, element.Name, nameMappingReference.ToString());
            throw new ArgumentException(message, nameof(element));
        }

        elementReference = element;
        nameMappingReference = resourceId;
    }

    void ICollection<KeyValuePair<ResourceID, Element>>.Add(KeyValuePair<ResourceID, Element> item) {
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
    
    public bool Remove(ResourceID resourceId, out Element output) {
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

    public bool Remove(string name, out Element output) {
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

    bool ICollection<KeyValuePair<ResourceID, Element>>.Remove(KeyValuePair<ResourceID, Element> item) {
        if (((ICollection<KeyValuePair<ResourceID, Element>>)_resources).Remove(item)) {
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
    
    bool ICollection<KeyValuePair<ResourceID, Element>>.Contains(KeyValuePair<ResourceID, Element> item) => _resources.Contains(item);
    
    public bool TryGetValue(ResourceID resourceId, out Element output) => _resources.TryGetValue(resourceId, out output);

    public Element this[ResourceID resourceId] {
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

    void ICollection<KeyValuePair<ResourceID, Element>>.CopyTo(KeyValuePair<ResourceID, Element>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<ResourceID, Element>>)_resources).CopyTo(array, arrayIndex);
    }

    [StackTraceHidden]
    private static void ValidateResourceId(ResourceID resourceId) {
        if (resourceId == ResourceID.Null) {
            throw new ArgumentException("Resource ID cannot be null or zero value.", nameof(resourceId));
        }
    }

    [StackTraceHidden]
    protected virtual void ValidateElement(Element element) {
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

    public Dictionary<ResourceID, Element>.Enumerator GetEnumerator() => _resources.GetEnumerator();

    IEnumerator<KeyValuePair<ResourceID, Element>> IEnumerable<KeyValuePair<ResourceID, Element>>.GetEnumerator() {
        return _resources.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly record struct Element(string Name, ImmutableArray<string> Tags, T Option);
}

file static class Constants {
    public static System.Buffers.SearchValues<char> ValidTagCharacters { get; } = 
        System.Buffers.SearchValues.Create("_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}
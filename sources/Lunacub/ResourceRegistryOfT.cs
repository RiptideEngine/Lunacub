using Caxivitual.Lunacub.Serialization;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceRegistryJsonConverterFactory))]
public class ResourceRegistry<TElement> 
    : IDictionary<ResourceID, TElement>,
      IReadOnlyDictionary<ResourceID, TElement> where TElement : IResourceRegistryElement 
{
    private readonly Dictionary<ResourceID, TElement> _resources = [];
    private readonly Dictionary<string, ResourceID> _nameMap = new(StringComparer.Ordinal);
    
    public int Count => _resources.Count;

    [ExcludeFromCodeCoverage] bool ICollection<KeyValuePair<ResourceID, TElement>>.IsReadOnly => false;
    
    public Dictionary<ResourceID, TElement>.KeyCollection Keys => _resources.Keys;
    public Dictionary<ResourceID, TElement>.ValueCollection Values => _resources.Values;
    
    ICollection<ResourceID> IDictionary<ResourceID, TElement>.Keys => _resources.Keys;
    ICollection<TElement> IDictionary<ResourceID, TElement>.Values => _resources.Values;
    
    IEnumerable<ResourceID> IReadOnlyDictionary<ResourceID, TElement>.Keys => _resources.Keys;
    IEnumerable<TElement> IReadOnlyDictionary<ResourceID, TElement>.Values => _resources.Values;

    internal IReadOnlyDictionary<string, ResourceID> NameMap => _nameMap;
    
    public void Add(ResourceID resourceId, TElement element) {
        ValidateElement(element);

        if (_resources.ContainsKey(resourceId)) {
            string message = string.Format(ExceptionMessages.ResourceIdAlreadyRegistered, resourceId.ToString());
            throw new ArgumentException(message, nameof(resourceId));
        }
        
        if (!string.IsNullOrEmpty(element.Name)) {
            ref var nameReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, element.Name, out bool exists);
            
            if (exists) {
                string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, element.Name, nameReference.ToString());
                throw new ArgumentException(message, nameof(element));
            }

            nameReference = resourceId;
        }
        
        _resources.Add(resourceId, element);
    }

    public bool TryAdd(ResourceID resourceId, in TElement element) {
        if (_resources.ContainsKey(resourceId)) return false;
        if (!IsValidElement(element)) return false;
        
        if (!string.IsNullOrEmpty(element.Name)) {
            ref var nameReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, element.Name, out bool exists);

            if (exists) return false;

            nameReference = resourceId;
        }
        
        _resources.Add(resourceId, element);

        return false;
    }

    void ICollection<KeyValuePair<ResourceID, TElement>>.Add(KeyValuePair<ResourceID, TElement> item) {
        ValidateElement(item.Value);

        (ResourceID resourceId, TElement element) = item;
        
        if (_resources.ContainsKey(resourceId)) {
            string message = string.Format(ExceptionMessages.ResourceIdAlreadyRegistered, resourceId.ToString());
            throw new ArgumentException(message, nameof(resourceId));
        }
        
        if (!string.IsNullOrEmpty(element.Name)) {
            ref var nameReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, element.Name, out bool exists);
            
            if (exists) {
                string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, element.Name, nameReference.ToString());
                throw new ArgumentException(message, nameof(item));
            }

            nameReference = resourceId;
        }

        _resources.Add(resourceId, element);
    }

    public bool Remove(ResourceID resourceId) => Remove(resourceId, out _);
    
    public bool Remove(ResourceID resourceId, [NotNullWhen(true)] out TElement? output) {
        if (_resources.Remove(resourceId, out output)) {
            if (!string.IsNullOrEmpty(output.Name)) {
                bool removal = _nameMap.Remove(output.Name);
                Debug.Assert(removal);
            }

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
            if (!string.IsNullOrEmpty(item.Value.Name)) {
                bool removal = _nameMap.Remove(item.Value.Name);
                Debug.Assert(removal);
            }

            return true;
        }

        return false;
    }
    
    public void Clear() {
        _resources.Clear();
        _nameMap.Clear();
    }
    
    public bool ContainsKey(ResourceID resourceId) => _resources.ContainsKey(resourceId);

    public bool ContainsName(ReadOnlySpan<char> name) => _nameMap.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(name);

    bool ICollection<KeyValuePair<ResourceID, TElement>>.Contains(KeyValuePair<ResourceID, TElement> item) {
        return _resources.Contains(item);
    }

    public bool TryGetValue(ResourceID resourceId, [NotNullWhen(true)] out TElement? output) {
        return _resources.TryGetValue(resourceId, out output);
    }
    
    public bool TryGetValue(ReadOnlySpan<char> name, out ResourceID id, [NotNullWhen(true)] out TElement? output) {
        if (_nameMap.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(name, out id)) {
            output = _resources[id];
            return true;
        }

        output = default;
        return false;
    }
    
    public bool TryGetValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out TElement? output) {
        return TryGetValue(name, out _, out output);
    }

    public TElement this[ResourceID resourceId] {
        get => _resources[resourceId];
        set {
            ValidateElement(value);
            
            if (!string.IsNullOrEmpty(value.Name)) {
                if (_nameMap.TryGetValue(value.Name, out ResourceID nameId)) {
                    string message = string.Format(ExceptionMessages.ResourceNameAlreadyRegistered, value.Name, nameId.ToString());
                    throw new ArgumentException(message, nameof(value));
                }
            }
            
            ref var elementReference = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourceId, out bool exists);
            
            if (exists && !string.IsNullOrEmpty(elementReference!.Name)) {
                bool removal = _nameMap.Remove(elementReference.Name);
                Debug.Assert(removal);
            }
            
            elementReference = value;

            if (!string.IsNullOrEmpty(value.Name)) {
                _nameMap.Add(value.Name, resourceId);
            }
        }
    }

    void ICollection<KeyValuePair<ResourceID, TElement>>.CopyTo(KeyValuePair<ResourceID, TElement>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<ResourceID, TElement>>)_resources).CopyTo(array, arrayIndex);
    }

    protected virtual bool IsValidElement(in TElement element) {
        if (typeof(TElement).IsClass && EqualityComparer<TElement>.Default.Equals(element, default)) return false;

        return true;
    }

    [StackTraceHidden]
    protected virtual void ValidateElement(in TElement element) {
        ArgumentNullException.ThrowIfNull(element);
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
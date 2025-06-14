namespace Caxivitual.Lunacub.Importing;

public sealed class DeserializationContext {
    private readonly Dictionary<ReferencePropertyKey, RequestingDependency> _requestingDependencies;
    internal IReadOnlyDictionary<ReferencePropertyKey, RequestingDependency> RequestingDependencies => _requestingDependencies;
    internal Dictionary<ReferencePropertyKey, object?>? References { get; set; }
    
    public Dictionary<object, object> ValueContainer { get; }

    internal DeserializationContext() {
        _requestingDependencies = [];
        ValueContainer = [];
    }

    public void RequestReference(ReferencePropertyKey propertyKey, ResourceID rid, Type resourceType) {
        if (resourceType.IsValueType || rid == ResourceID.Null || resourceType.IsGenericTypeDefinition) return;
        
        _requestingDependencies.Add(propertyKey, new(rid, resourceType));
    }
    
    public void RequestReference<T>(ReferencePropertyKey propertyKey, ResourceID rid) where T : class {
        if (rid == ResourceID.Null) return;
        
        _requestingDependencies.Add(propertyKey, new(rid, typeof(T)));
    }
    
    public T? GetReference<T>(ReferencePropertyKey propertyKey) where T : class {
        if (References != null && References.TryGetValue(propertyKey, out object? dependency) && dependency is T t) {
            return t;
        }

        return null;
    }

    public readonly record struct RequestingDependency(ResourceID Rid, Type Type);
}
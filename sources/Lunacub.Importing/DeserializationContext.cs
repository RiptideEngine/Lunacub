using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

public sealed class DeserializationContext {
    private readonly Dictionary<ReferencePropertyKey, RequestingDependency> _requestingReferences;
    internal IReadOnlyDictionary<ReferencePropertyKey, RequestingDependency> RequestingReferences => _requestingReferences;
    internal Dictionary<ReferencePropertyKey, object?>? References { get; set; }
    
    public Dictionary<object, object> ValueContainer { get; }
    
    public ILogger Logger { get; }

    internal DeserializationContext(ILogger logger) {
        _requestingReferences = [];
        ValueContainer = [];
        Logger = logger;
    }

    public void RequestReference(ReferencePropertyKey propertyKey, ResourceID rid, Type resourceType) {
        if (resourceType.IsValueType || rid == ResourceID.Null || resourceType.IsGenericTypeDefinition) return;
        
        _requestingReferences.Add(propertyKey, new(rid, resourceType));
    }
    
    public void RequestReference<T>(ReferencePropertyKey propertyKey, ResourceID rid) where T : class {
        if (rid == ResourceID.Null) return;
        
        _requestingReferences.Add(propertyKey, new(rid, typeof(T)));
    }
    
    public T? GetReference<T>(ReferencePropertyKey propertyKey) where T : class {
        if (References != null && References.TryGetValue(propertyKey, out object? dependency) && dependency is T t) {
            return t;
        }

        return null;
    }

    public readonly record struct RequestingDependency(ResourceID ResourceId, Type Type);
}
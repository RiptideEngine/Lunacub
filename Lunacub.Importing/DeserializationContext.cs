namespace Caxivitual.Lunacub.Importing;

public sealed class DeserializationContext {
    private readonly Dictionary<string, RequestingDependency> _requestingDependencies;
    internal IReadOnlyDictionary<string, RequestingDependency> RequestingDependencies => _requestingDependencies;
    internal Dictionary<string, object?>? Dependencies { get; set; }

    internal DeserializationContext() {
        _requestingDependencies = new(StringComparer.Ordinal);
    }

    public void RequestReference(string property, ResourceID rid, Type resourceType) {
        if (resourceType.IsValueType) return;
        
        _requestingDependencies.Add(property, new(rid, resourceType));
    }
    
    public void RequestReference<T>(string property, ResourceID rid) {
        RequestReference(property, rid, typeof(T));
    }
    
    public T? GetReference<T>(ReadOnlySpan<char> property) where T : class {
        if (Dependencies != null && Dependencies.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(property, out object? dependency) && dependency is T t) {
            return t;
        }

        return null;
    }

    public readonly record struct RequestingDependency(ResourceID Rid, Type Type);
}
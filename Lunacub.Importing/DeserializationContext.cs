namespace Caxivitual.Lunacub.Importing;

public sealed class DeserializationContext {
    private readonly Dictionary<string, object> _dependencies;

    public IReadOnlyDictionary<string, object> Dependencies => _dependencies;
    
    internal DeserializationContext() {
        _dependencies = new(StringComparer.Ordinal);
    }

    public void RequestDependency(string property, ResourceID rid, Type resourceType) {
        
    }

    public void RequestDependency(string property, string path, Type resourceType) {
        
    }

    public void RequestDependency<T>(string property, ResourceID rid) where T : class {
        
    }
    
    public void RequestDependency<T>(string property, string path) where T : class {
        
    }

    public T? GetDependency<T>(ReadOnlySpan<char> property) where T : class {
        if (_dependencies.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(property, out object? dependency) && dependency is T t) {
            return t;
        }

        return null;
    }
}
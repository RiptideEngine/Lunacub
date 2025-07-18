using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
public readonly struct DeserializationContext {
    public RequestingReferences RequestingReferences { get; }
    public ILogger Logger { get; }
    public Dictionary<object, object> ValueContainer { get; }

    internal DeserializationContext(ILogger logger) {
        Logger = logger;
        RequestingReferences = new();
        ValueContainer = [];
    }

    public void RequestReference(ReferencePropertyKey key, RequestingReferences.RequestingReference requesting) {
        RequestingReferences.Add(key, requesting);
    }

    public bool RemoveRequestingReference(ReferencePropertyKey key) {
        return RequestingReferences.Remove(key);
    }

    public ResourceHandle GetReference(ReferencePropertyKey key) => RequestingReferences.GetReference(key);
    
    public bool TryGetReference(ReferencePropertyKey key, out ResourceHandle handle) {
        return RequestingReferences.TryGetReference(key, out handle);
    }
}
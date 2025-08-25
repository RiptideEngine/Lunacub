using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents the context used during the resource deserialization process, providing access to some environment properties.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct DeserializationContext {
    public RequestingReferences RequestingReferences { get; }
    
    /// <summary>
    /// Gets the <see cref="ILogger"/> instance used for debugging and reporting.
    /// </summary>
    public ILogger Logger { get; }
    
    
    
    /// <summary>
    /// Gets the storage that used for storing custom data between deserialization stages.
    /// </summary>
    public Dictionary<object, object?> ValueContainer { get; }

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

    public void ReleaseReference(ReferencePropertyKey key) {
        RequestingReferences.ReleaseReference(key);
    }
}
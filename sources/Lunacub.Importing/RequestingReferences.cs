namespace Caxivitual.Lunacub.Importing;

public sealed class RequestingReferences {
    private bool _enableRequest;
    
    private readonly Dictionary<ReferencePropertyKey, RequestingReference> _requestingReferences;
    private IReadOnlyDictionary<ReferencePropertyKey, ResourceHandle>? _references;
    
    public int Count => _requestingReferences.Count;
    
    internal IReadOnlyDictionary<ReferencePropertyKey, RequestingReference> Requesting => _requestingReferences;
    
    public RequestingReferences() {
        _enableRequest = true;
        _requestingReferences = [];
    }

    internal void DisableRequest() {
        _enableRequest = false;
    }

    public void Add(ReferencePropertyKey key, RequestingReference requesting) {
        if (!_enableRequest) {
            throw new InvalidOperationException("Reference requesting is disabled.");
        }

        if (requesting.ResourceId == ResourceID.Null) return;

        _requestingReferences[key] = requesting;
    }

    public bool Remove(ReferencePropertyKey key) {
        if (!_enableRequest) {
            throw new InvalidOperationException("Reference requesting is disabled.");
        }

        return _requestingReferences.Remove(key);
    }

    public ResourceHandle GetReference(ReferencePropertyKey key) {
        if (_references == null) {
            throw new InvalidOperationException("Reference retrieving is disabled.");
        }

        return _references[key];
    }

    public bool TryGetReference(ReferencePropertyKey key, out ResourceHandle handle) {
        if (_references == null) {
            throw new InvalidOperationException("Reference retrieving is disabled.");
        }
        
        return _references.TryGetValue(key, out handle);
    }

    internal void SetReferences(Dictionary<ReferencePropertyKey, ResourceHandle> references) {
        _references = references;
    }
    
    public readonly record struct RequestingReference(ResourceID ResourceId);
}
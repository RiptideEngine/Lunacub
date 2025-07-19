namespace Caxivitual.Lunacub.Importing;

public sealed class RequestingReferences {
    private bool _enableRequest;
    
    private readonly Dictionary<ReferencePropertyKey, RequestingReference> _requestingReferences;
    public Dictionary<ReferencePropertyKey, ResourceHandle>? References { get; set; }
    
    public int Count => _requestingReferences.Count;
    
    internal IReadOnlyDictionary<ReferencePropertyKey, RequestingReference> Requesting => _requestingReferences;

    private readonly HashSet<ReferencePropertyKey> _releaseReferences = [];
    internal IReadOnlySet<ReferencePropertyKey> ReleasedReferences => _releaseReferences;
    
    internal RequestingReferences() {
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
        if (References == null) {
            throw new InvalidOperationException("Reference retrieving is disabled.");
        }

        if (_releaseReferences.Contains(key)) return default;

        return References[key];
    }

    public bool TryGetReference(ReferencePropertyKey key, out ResourceHandle handle) {
        if (References == null) {
            throw new InvalidOperationException("Reference retrieving is disabled.");
        }

        if (_releaseReferences.Contains(key)) {
            handle = default;
            return false;
        }
        
        return References.TryGetValue(key, out handle);
    }

    public void ReleaseReference(ReferencePropertyKey key) {
        if (References == null) {
            throw new InvalidOperationException("Reference releasing is disabled.");
        }

        if (!References.ContainsKey(key)) return;

        _releaseReferences.Add(key);
    }

    public readonly record struct RequestingReference(ResourceID ResourceId);
}
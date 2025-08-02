namespace Caxivitual.Lunacub.Building.Collections;

public sealed class ProceduralResourceCollection : IEnumerable<ProceduralResourceCollection.Request> {
    private readonly List<Request> _requests;
    private readonly LibraryID _libraryId;
    
    public int Count => _requests.Count;

    internal ProceduralResourceCollection(LibraryID libraryId) {
        _requests = [];
        _libraryId = libraryId;
    }

    public void Add(BuildingProceduralResource resource, out ResourceAddress outputAddress) {
        // TODO: Validate resource.
        
        // Since Guid v7 is time-based, it should theoretically be impossible to collide with the environment resource ids.
        ResourceID id = Unsafe.BitCast<Guid, ResourceID>(Guid.CreateVersion7());
        _requests.Add(new(id, resource));
        outputAddress = new(_libraryId, id);
    }
    
    public bool Remove(ResourceID resourceId, out BuildingProceduralResource removedResource) {
        foreach (var request in _requests) {
            if (request.ResourceId == resourceId) {
                removedResource = request.Resource;
                return true;
            }
        }

        removedResource = default;
        return false;
    }

    public bool Contains(ResourceID resourceId) {
        foreach (var request in _requests) {
            if (request.ResourceId == resourceId) return true;
        }

        return false;
    }

    public void Clear() => _requests.Clear();
    
    public List<Request>.Enumerator GetEnumerator() => _requests.GetEnumerator();
    
    IEnumerator<Request> IEnumerable<Request>.GetEnumerator() => _requests.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _requests.GetEnumerator();
    
    public readonly record struct Request(ResourceID ResourceId, BuildingProceduralResource Resource);
}
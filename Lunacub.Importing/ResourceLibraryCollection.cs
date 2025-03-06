using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

public sealed class ResourceLibraryCollection : Collection<ResourceLibrary> {
    private readonly Dictionary<string, ResourceID> _pathToIDMap;
    
    internal IReadOnlyDictionary<string, ResourceID> PathToIDMap => _pathToIDMap;

    internal ResourceLibraryCollection() {
        _pathToIDMap = [];
    }
    
    public bool Remove(Guid id) {
        for (int i = 0; i < Count; i++) {
            if (this[i].Id == id) {
                RemoveAt(i);
                return true;
            }
        }
        
        return false;
    }
    
    protected override void InsertItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ValidateLibrary(item);
        
        base.InsertItem(index, item);

        foreach ((ResourceID id, string path) in item) {
            _pathToIDMap.Add(path, id);
        }
    }

    protected override void SetItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ValidateLibrary(item);
        
        base.SetItem(index, item);
    }

    [StackTraceHidden]
    private void ValidateLibrary(ResourceLibrary library) {
        foreach (var enumerate in this) {
            if (enumerate.Id == library.Id) {
                throw new ArgumentException($"An instance of {nameof(ResourceLibrary)} with ID {library.Id} already present.");
            }

            KeyValuePair<ResourceID, string> ridCollision = library.UnionBy(enumerate, kvp => kvp.Key).FirstOrDefault();
            if (ridCollision.Key != ResourceID.Null) {
                throw new ArgumentException($"Provided {nameof(ResourceLibrary)} ID '{library.Id}' contains Resource with ID {ridCollision.Key}, which is already registered in {nameof(ResourceLibrary)} ID '{enumerate.Id}'.");
            }
            
            KeyValuePair<ResourceID, string> pathCollision = library.UnionBy(enumerate, kvp => kvp.Value).FirstOrDefault();
            if (pathCollision.Key != ResourceID.Null) {
                throw new ArgumentException($"Provided {nameof(ResourceLibrary)} ID '{library.Id}' contains Resource with Path '{pathCollision.Value}', which is already registered in {nameof(ResourceLibrary)} ID '{enumerate.Id}'.");
            }
        }
    }
}
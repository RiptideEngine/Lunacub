namespace Caxivitual.Lunacub.Building.Collections;

public sealed class LibraryProceduralSchematic : LibraryResourceDictionary<List<ProceduralResourceSchematicInfo>> {
    public void Add(ResourceID resourceId, ProceduralResourceSchematicInfo info) {
        ref var collection = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, resourceId, out bool exists);
        
        if (!exists) {
            collection = [];
        }
        
        collection!.Add(info);
    }
    
    public bool Remove(ResourceID resourceId, ResourceID id) {
        if (_dict.TryGetValue(resourceId, out var collection)) {
            for (int i = 0; i < collection.Count; i++) {
                if (collection[i].ResourceId == id) {
                    collection.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    public void Clear(ResourceID resourceId) {
        if (_dict.TryGetValue(resourceId, out var collection)) {
            collection.Clear();
        }
    }
}
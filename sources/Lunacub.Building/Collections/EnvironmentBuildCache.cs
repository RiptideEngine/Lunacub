using Caxivitual.Lunacub.Building.Incremental;
using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Building.Collections;

public sealed class EnvironmentBuildCache : LibraryIdentityDictionary<LibraryBuildCache> {
    public void SetIncrementalInfo(LibraryID libraryId, ResourceID resourceId, BuildCache buildCache) {
        ref var libraryIncrementalInfos = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, libraryId, out bool exists);

        if (!exists) {
            libraryIncrementalInfos = [];
        }

        libraryIncrementalInfos![resourceId] = buildCache;
    }
    
    public void SetIncrementalInfo(ResourceAddress address, BuildCache buildCache) {
        ref var libraryIncrementalInfos = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, address.LibraryId, out bool exists);

        if (!exists) {
            libraryIncrementalInfos = [];
        }

        libraryIncrementalInfos![address.ResourceId] = buildCache;
    }
}
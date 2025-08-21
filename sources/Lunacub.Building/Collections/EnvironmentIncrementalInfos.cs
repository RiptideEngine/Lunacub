using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Building.Collections;

public sealed class EnvironmentIncrementalInfos : LibraryIdentityDictionary<LibraryIncrementalInfos> {
    public void SetIncrementalInfo(LibraryID libraryId, ResourceID resourceId, IncrementalInfo incrementalInfo) {
        ref var libraryIncrementalInfos = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, libraryId, out bool exists);

        if (!exists) {
            libraryIncrementalInfos = [];
        }

        libraryIncrementalInfos![resourceId] = incrementalInfo;
    }
    
    public void SetIncrementalInfo(ResourceAddress address, IncrementalInfo incrementalInfo) {
        ref var libraryIncrementalInfos = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, address.LibraryId, out bool exists);

        if (!exists) {
            libraryIncrementalInfos = [];
        }

        libraryIncrementalInfos![address.ResourceId] = incrementalInfo;
    }
}
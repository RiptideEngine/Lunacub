namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryBuildCacheIO : IBuildCacheIO {
    public EnvironmentBuildCache BuildCache { get; } = [];
    
    public void CollectIncrementalInfos(EnvironmentBuildCache receiver) {
        foreach ((var libraryId, var libraryIncrementalInfos) in BuildCache) {
            LibraryBuildCache receiverLibraryBuildCache = [];

            foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                receiverLibraryBuildCache.Add(resourceId, incrementalInfos);
            }
            
            receiver.Add(libraryId, receiverLibraryBuildCache);
        }
    }

    public void FlushBuildCaches(EnvironmentBuildCache envBuildCache) {
        BuildCache.Clear();
        
        foreach ((var libraryId, var libraryIncrementalInfos) in envBuildCache) {
            if (BuildCache.TryGetValue(libraryId, out var receiverLibraryIncrementalInfos)) {
                foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                    receiverLibraryIncrementalInfos[resourceId] = incrementalInfos;
                }
            } else {
                receiverLibraryIncrementalInfos = [];

                foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                    receiverLibraryIncrementalInfos.Add(resourceId, incrementalInfos);
                }
            
                BuildCache.Add(libraryId, receiverLibraryIncrementalInfos);
            }
        }
    }
}
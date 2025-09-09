namespace Caxivitual.Lunacub.Building.Core;

public sealed class VoidBuildCacheSink : IBuildCacheIO {
    public static VoidBuildCacheSink Instance { get; } = new();
    
    internal VoidBuildCacheSink() { }

    public void CollectIncrementalInfos(EnvironmentBuildCache receiver) { }
    public void FlushBuildCaches(EnvironmentBuildCache buildCache) { }
}
namespace Caxivitual.Lunacub.Building;

public abstract class OutputSystem {
    public abstract void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver);
    public abstract void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports);

    public abstract DateTime? GetResourceLastBuildTime(ResourceID rid);

    public abstract Stream CreateDestinationStream(ResourceID rid);
}
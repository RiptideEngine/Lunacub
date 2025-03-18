namespace Caxivitual.Lunacub.Building;

public abstract class OutputSystem {
    public abstract void CollectReports(IDictionary<ResourceID, BuildingReport> receiver);
    public abstract void FlushReports(IReadOnlyDictionary<ResourceID, BuildingReport> reports);

    public abstract DateTime? GetResourceLastBuildTime(ResourceID rid);

    public abstract Stream CreateDestinationStream(ResourceID rid);
}
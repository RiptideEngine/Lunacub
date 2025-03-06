namespace Caxivitual.Lunacub.Building;

public abstract class BuildOutput {
    public abstract void CollectReports(IDictionary<ResourceID, BuildingReport> receiver);
    public abstract void FlushReports(IReadOnlyDictionary<ResourceID, BuildingReport> reports);

    public abstract string GetBuildDestination(ResourceID rid);
    public abstract Stream GetResourceOutputStream(string buildDestination);
}
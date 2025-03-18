using Caxivitual.Lunacub.Building.Serialization;

namespace Caxivitual.Lunacub.Building;

[JsonConverter(typeof(BuildingReportConverter))]
public struct BuildingReport {
    public DateTime SourceLastWriteTime;
    public IReadOnlySet<ResourceID> Dependencies;
    public BuildingOptions Options;
}
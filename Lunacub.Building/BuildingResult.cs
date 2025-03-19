namespace Caxivitual.Lunacub.Building;

public readonly struct BuildingResult {
    public readonly DateTime BuildStartTime;
    public readonly DateTime BuildFinishTime;
    public readonly IReadOnlyDictionary<ResourceID, ResourceBuildingResult>? ResourceResults;

    internal BuildingResult(DateTime startTime, DateTime finishTime, IReadOnlyDictionary<ResourceID, ResourceBuildingResult>? resourceResults) {
        BuildStartTime = startTime;
        BuildFinishTime = finishTime;
        ResourceResults = resourceResults;
    }
}
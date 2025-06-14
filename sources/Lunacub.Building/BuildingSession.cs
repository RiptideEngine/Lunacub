namespace Caxivitual.Lunacub.Building;

internal readonly struct BuildingSession {
    public readonly Dictionary<ResourceID, ResourceBuildingResult> Results;
    public readonly Dictionary<ResourceID, ContentRepresentation> ImportedRepresentations;

    public BuildingSession() {
        Results = [];
        ImportedRepresentations = [];
    }
}
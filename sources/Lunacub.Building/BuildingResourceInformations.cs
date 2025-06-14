namespace Caxivitual.Lunacub.Building;

public readonly record struct BuildingResourceInformations(IReadOnlySet<ResourceID> Dependencies, IReadOnlyDictionary<ProceduralResourceID, BuildingOptions> ProceduralResources);
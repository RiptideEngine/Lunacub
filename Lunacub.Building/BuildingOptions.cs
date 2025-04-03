namespace Caxivitual.Lunacub.Building;

public readonly record struct BuildingOptions(string ImporterName, string? ProcessorName, IImportOptions? Options = null);
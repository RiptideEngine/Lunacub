namespace Caxivitual.Lunacub.Building.Incremental;

public readonly record struct ComponentVersions(
    string? ImporterVersion,
    string? ProcessorVersion
);
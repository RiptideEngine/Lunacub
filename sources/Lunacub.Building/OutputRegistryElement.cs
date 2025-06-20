using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building;

public readonly record struct OutputRegistryElement(string Name, ImmutableArray<string> Tags);
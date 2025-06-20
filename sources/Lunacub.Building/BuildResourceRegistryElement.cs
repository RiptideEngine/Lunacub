using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building;

public readonly record struct BuildResourceRegistryElement(string Name, ImmutableArray<string> Tags, BuildingResource Option) : IRegistryElement;
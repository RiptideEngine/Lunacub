using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

public readonly record struct PrimitiveRegistryElement(string Name, ImmutableArray<string> Tags) : IRegistryElement;
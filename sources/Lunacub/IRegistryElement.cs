using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

public interface IRegistryElement {
    string Name { get; }
    ImmutableArray<string> Tags { get; }
}
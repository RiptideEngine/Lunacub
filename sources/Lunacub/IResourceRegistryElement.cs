using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

public interface IResourceRegistryElement {
    string Name { get; }
    ImmutableArray<string> Tags { get; }
}
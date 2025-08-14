using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

public interface IResourceRegistryElement {
    string? Name { get; }
    TagCollection Tags { get; }
}
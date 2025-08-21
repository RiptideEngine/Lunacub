namespace Caxivitual.Lunacub;

public interface IResourceRegistryElement {
    string? Name { get; }
    TagCollection Tags { get; }
}
namespace Caxivitual.Lunacub.Extensions;

public static class ResourceRegistryExtensions {
    public static ResourceRegistry<T> AddElement<T>(this ResourceRegistry<T> registry, ResourceID resourceId, T element) where T : IResourceRegistryElement {
        registry.Add(resourceId, element);
        return registry;
    }
}
namespace Caxivitual.Lunacub.Building.Extensions;

public static class BuildResourceLibraryExtensions {
    public static BuildResourceLibrary AddRegistryElement(this BuildResourceLibrary library, ResourceID resourceId, ResourceRegistry.Element<BuildingResource> element) {
        library.Registry.Add(resourceId, element);
        return library;
    }
}
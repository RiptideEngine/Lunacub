namespace Caxivitual.Lunacub.Importing.Extensions;

public static class ImportResourceLibraryExtensions {
    public static ImportResourceLibrary AddRegistryElement(this ImportResourceLibrary library, ResourceID resourceId, ResourceRegistry.Element element) {
        library.Registry.Add(resourceId, element);
        return library;
    }
}
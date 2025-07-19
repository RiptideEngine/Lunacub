namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    internal ResourceCache.ElementContainer? GetResourceContainer(ResourceID resourceId) {
        return _importDispatcher.Cache.Get(resourceId);
    }

    internal ResourceCache.ElementContainer? GetResourceContainer(object resource) {
        return _importDispatcher.Cache.Get(resource);
    }
}
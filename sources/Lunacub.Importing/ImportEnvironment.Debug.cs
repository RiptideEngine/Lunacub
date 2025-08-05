namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    internal ResourceCache.ElementContainer? GetResourceContainer(ResourceAddress address) {
        return _importDispatcher.Cache.Get(address);
    }

    internal ResourceCache.ElementContainer? GetResourceContainer(object resource) {
        return _importDispatcher.Cache.Get(resource);
    }
}
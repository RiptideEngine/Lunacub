namespace Caxivitual.Lunacub.Building;

public abstract class BuildResourceLibrary : ResourceLibrary<BuildingResource> {
    protected BuildResourceLibrary() : base([]) {
    }

    public DateTime GetResourceLastWriteTime(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry<BuildingResource>.Element element)) return default;

        return GetResourceLastWriteTimeCore(resourceId, element.Option);
    }

    protected abstract DateTime GetResourceLastWriteTimeCore(ResourceID resourceId, BuildingResource resource);
}
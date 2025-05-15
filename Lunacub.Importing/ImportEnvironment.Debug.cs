namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    internal uint GetResourceReferenceCount(ResourceID rid) => _resourceCache.GetReferenceCount(rid);
    internal ValueTask<uint> GetResourceReferenceCountAsync(ResourceID rid) => _resourceCache.GetReferenceCountAsync(rid);

    internal ResourceStatus GetResourceStatus(ResourceID rid) => _resourceCache.GetResourceStatus(rid);
}
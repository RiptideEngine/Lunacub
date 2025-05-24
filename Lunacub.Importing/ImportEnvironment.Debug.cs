namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    /// <summary>
    /// Gets the reference count of the resource associates with a <see cref="ResourceID"/>, used for debugging purpose.
    /// </summary>
    /// <param name="rid">The Id associates with the resource.</param>
    /// <returns>The reference count of the resource associates with <paramref name="rid"/>.</returns>
    internal uint GetResourceReferenceCount(ResourceID rid) => _resourceCache.GetReferenceCount(rid);
    
    /// <summary>
    /// Gets the reference count of the resource associates with a <see cref="ResourceID"/> asynchronously, used for
    /// debugging purpose.
    /// </summary>
    /// <param name="rid">The Id associates with the resource.</param>
    /// <returns>The reference count of the resource associates with <paramref name="rid"/>.</returns>
    internal ValueTask<uint> GetResourceReferenceCountAsync(ResourceID rid) => _resourceCache.GetReferenceCountAsync(rid);

    /// <summary>
    /// Gets the resource status of the resource associates with a <see cref="ResourceID"/>, used for debugging purpose.
    /// </summary>
    /// <param name="rid">The Id associates with the resource.</param>
    /// <returns>The resource status of the resource associates with <paramref name="rid"/>.</returns>
    /// <seealso cref="ResourceStatus"/>
    internal ResourceStatus GetResourceStatus(ResourceID rid) {
        if (!Libraries.ContainResource(rid)) return ResourceStatus.Unregistered;
        
        return _resourceCache.GetResourceStatus(rid);
    }
}
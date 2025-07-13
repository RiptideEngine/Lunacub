namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that writes the compiled binary of a resource to a persistent storage.
/// </summary>
public abstract class OutputSystem {
    /// <summary>
    /// Collects all the previously created <see cref="IncrementalInfo"/> into a container.
    /// </summary>
    /// <param name="receiver">A dictionary that receives the <see cref="IncrementalInfo"/> for each resource.</param>
    public abstract void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver);
    
    /// <summary>
    /// Flushes all the storing <see cref="IncrementalInfo"/> to a persistent storage for later use.
    /// </summary>
    /// <param name="reports">
    /// A readonly dictionary that contains all the <see cref="IncrementalInfo"/> of currently resource building session.
    /// </param>
    public abstract void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports);
    
    /// <summary>
    /// Gets the last build time of a resource.
    /// </summary>
    /// <param name="rid">The resource to get the last build time of.</param>
    /// <returns>
    /// The last build time of a resource, <see langword="null"/> if the resource has never been built before.
    /// </returns>
    public abstract DateTime? GetResourceLastBuildTime(ResourceID rid);
    
    /// <summary>
    /// Flushes the compiled binary of a resource.
    /// </summary>
    /// <param name="sourceStream">Stream contains the compiled resource binary.</param>
    /// <param name="rid">The Id of the compiled resource.</param>
    public abstract void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid);

    /// <summary>
    /// Flushes the successfully built resources.
    /// </summary>
    /// <param name="registry">The registry contains the identifiers of successfully built resources.</param>
    public abstract void OutputResourceRegistry(ResourceRegistry<ResourceRegistry.Element> registry);
}
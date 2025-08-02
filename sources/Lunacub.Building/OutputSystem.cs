using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that writes the compiled binary of a resource to a persistent storage.
/// </summary>
public abstract class OutputSystem {
    /// <summary>
    /// Collects all the previously created <see cref="IncrementalInfo"/> into a container.
    /// </summary>
    /// <param name="receiver">The container that receives the <see cref="IncrementalInfo"/> for each library.</param>
    public abstract void CollectIncrementalInfos(EnvironmentIncrementalInfos receiver);
    
    /// <summary>
    /// Flushes all the storing <see cref="IncrementalInfo"/> to a persistent storage for later use.
    /// </summary>
    /// <param name="incrementalInfos">
    /// The container that contains all the <see cref="IncrementalInfo"/> generated from building sessions.
    /// </param>
    public abstract void FlushIncrementalInfos(EnvironmentIncrementalInfos incrementalInfos);
    
    /// <summary>
    /// Gets the last build time of a resource.
    /// </summary>
    /// <param name="address">The resource to get the last build time of.</param>
    /// <returns>
    /// The last build time of a resource, <see langword="null"/> if the resource has never been built before.
    /// </returns>
    public abstract DateTime? GetResourceLastBuildTime(ResourceAddress address);

    /// <summary>
    /// Flushes the compiled binary of a resource.
    /// </summary>
    /// <param name="sourceStream">Stream contains the compiled resource binary.</param>
    /// <param name="address">The address of resource that being flushed.</param>
    public abstract void CopyCompiledResourceOutput(Stream sourceStream, ResourceAddress address);

    /// <summary>
    /// Flushes the resource registry of a resource library.
    /// </summary>
    /// <param name="registry">The registry contains the identifiers of successfully built resources.</param>
    /// <param name="libraryId">The library id.</param>
    public abstract void OutputLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId);
}
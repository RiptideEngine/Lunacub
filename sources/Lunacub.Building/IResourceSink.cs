namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents an output endpoint for compiled resources.
/// </summary>
public interface IResourceSink {
    /// <summary>
    /// Flushes the compiled binary of a resource.
    /// </summary>
    /// <param name="sourceStream">Stream contains the compiled resource binary.</param>
    /// <param name="address">The address of resource that being flushed.</param>
    void FlushCompiledResource(Stream sourceStream, ResourceAddress address);
    
    /// <summary>
    /// Flushes the resource registry of a resource library.
    /// </summary>
    /// <param name="registry">The registry contains the identifiers of successfully built resources.</param>
    /// <param name="libraryId">The library id.</param>
    void FlushLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId);
}
using Caxivitual.Lunacub.Building.Collections;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the processing process, providing access to import options.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct ProcessingContext {
    /// <summary>
    /// Gets the <see cref="BuildEnvironment"/> instance responsible for the processing process.
    /// </summary>
    public BuildEnvironment Environment { get; }
    
    /// <summary>
    /// Gets the address of the currently processing resource.
    /// </summary>
    public ResourceAddress ResourceAddress { get; }

    /// <summary>
    /// Gets user-provided options that can be used during the processing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    /// <summary>
    /// Gets the dependency resources requested from importing stage.
    /// </summary>
    /// <seealso cref="Importer.ExtractDependencies"/>
    public IReadOnlyDictionary<ResourceAddress, ContentRepresentation> Dependencies { get; }
    
    /// <summary>
    /// Gets the procedural resources container generated during processing stage.
    /// </summary>
    public Dictionary<ProceduralResourceID, BuildingProceduralResource> ProceduralResources { get; }
    
    /// <summary>
    /// Gets the <see cref="ILogger"/> instance used for debugging and reporting.
    /// </summary>
    public ILogger Logger { get; }
    
    internal ProcessingContext(
        BuildEnvironment environment,
        ResourceAddress resourceAddress,
        IImportOptions? options,
        IReadOnlyDictionary<ResourceAddress, ContentRepresentation> dependencies,
        ILogger logger
    ) {
        Environment = environment;
        ResourceAddress = resourceAddress;
        Options = options;
        Dependencies = dependencies;
        ProceduralResources = [];
        Logger = logger;
    }
}
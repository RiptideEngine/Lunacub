using Caxivitual.Lunacub.Building.Collections;
using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the processing process, providing access to import options.
/// </summary>
public sealed class ProcessingContext {
    /// <summary>
    /// Gets the <see cref="BuildEnvironment"/> instance responsible for the processing process.
    /// </summary>
    public BuildEnvironment Environment { get; }

    /// <summary>
    /// Gets user-provided options that can be used during the processing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    /// <summary>
    /// Gets the dependency resources requested from importing stage.
    /// </summary>
    /// <seealso cref="Importer.ExtractDependencies"/>
    public IReadOnlyDictionary<ResourceID, ContentRepresentation> Dependencies { get; }
    
    /// <summary>
    /// Gets the dictionary that stores the generated resources to build in next pass.
    /// </summary>
    public Dictionary<ProceduralResourceID, BuildingProceduralResource> ProceduralResources { get; }
    
    /// <summary>
    /// Gets the <see cref="ILogger"/> instance used for debugging and reporting.
    /// </summary>
    public ILogger Logger { get; }
    
    internal ProcessingContext(BuildEnvironment environment, IImportOptions? options, IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies, ILogger logger) {
        Environment = environment;
        Options = options;
        Dependencies = dependencies;
        Logger = logger;
        ProceduralResources = [];
    }
}
using Caxivitual.Lunacub.Building.Collections;

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
    /// <seealso cref="Importer.GetDependencies"/>
    public IReadOnlyDictionary<ResourceID, ContentRepresentation> Dependencies { get; }

    internal ProcessingContext(BuildEnvironment environment, IImportOptions? options, IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies) {
        Environment = environment;
        Options = options;
        Dependencies = dependencies;
    }
}
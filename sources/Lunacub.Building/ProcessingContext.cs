using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the processing process, providing access to import options.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct ProcessingContext {
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
    public IReadOnlyDictionary<ResourceAddress, object> Dependencies { get; }
    
    /// <summary>
    /// Gets the procedural resources container generated during processing stage.
    /// </summary>
    public ProceduralResourceCollection ProceduralResources { get; }
    
    private readonly BuildEnvironment _environment;

    /// <summary>
    /// Gets the logger used for debugging and logging purpose.
    /// </summary>
    public ILogger Logger => _environment.Logger;
    
    /// <summary>
    /// Gets the dictionary of environment variables associates with the build environment.
    /// </summary>
    public IReadOnlyDictionary<object, object> EnvironmentVariables => _environment.EnvironmentVariables;
    
    internal ProcessingContext(
        BuildEnvironment environment,
        ResourceAddress resourceAddress,
        IImportOptions? options,
        IReadOnlyDictionary<ResourceAddress, object> dependencies
    ) {
        ResourceAddress = resourceAddress;
        Options = options;
        Dependencies = dependencies;
        ProceduralResources = new(resourceAddress.LibraryId);
        _environment = environment;
    }
}
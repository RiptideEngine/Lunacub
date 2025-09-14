namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options and other properties.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct ImportingContext {
    /// <summary>
    /// Gets the address of the requested resource.
    /// </summary>
    public ResourceAddress ResourceAddress { get; }
    
    /// <summary>
    /// User-provided options that can be used during the importing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }

    private readonly BuildEnvironment _environment;

    /// <summary>
    /// Gets the logger used for debugging and logging purpose.
    /// </summary>
    public ILogger Logger => _environment.Logger;
    
    /// <summary>
    /// Gets the dictionary of environment variables associates with the build environment.
    /// </summary>
    public IReadOnlyDictionary<object, object> EnvironmentVariables => _environment.EnvironmentVariables;
    
    internal ImportingContext(ResourceAddress resourceAddress, IImportOptions? options, BuildEnvironment environment) {
        ResourceAddress = resourceAddress;
        Options = options;
        _environment = environment;
    }
}
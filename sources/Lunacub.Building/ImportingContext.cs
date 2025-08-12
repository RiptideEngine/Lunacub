namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options and other properties.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct ImportingContext {
    /// <summary>
    /// User-provided options that can be used during the importing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    /// <summary>
    /// Gets the logger used for debugging and printing purpose.
    /// </summary>
    public ILogger Logger { get; }
    
    internal ImportingContext(IImportOptions? options, ILogger logger) {
        Options = options;
        Logger = logger;
    }
}
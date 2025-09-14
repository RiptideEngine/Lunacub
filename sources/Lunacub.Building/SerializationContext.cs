using Microsoft.IO;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the resource compilation process, providing access to import options.
/// </summary>
public readonly struct SerializationContext {
    /// <summary>
    /// User-provided options that can be used during the processing process.
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

    /// <summary>
    /// Gets the <see cref="RecyclableMemoryStreamManager"/> instance that associates with the <see cref="BuildEnvironment"/>.
    /// </summary>
    public RecyclableMemoryStreamManager MemoryStreamManager => _environment.MemoryStreamManager;

    internal SerializationContext(IImportOptions? options, BuildEnvironment environment) {
        Options = options;
        _environment = environment;
    }
}
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
    
    /// <summary>
    /// Gets the <see cref="ILogger"/> instance used for debugging and reporting.
    /// </summary>
    public ILogger Logger { get; }
    
    /// <summary>
    /// Gets the <see cref="RecyclableMemoryStreamManager"/> instance that associates with the <see cref="BuildEnvironment"/>.
    /// </summary>
    public RecyclableMemoryStreamManager MemoryStreamManager { get; }

    internal SerializationContext(IImportOptions? options, ILogger logger, RecyclableMemoryStreamManager memoryStreamManager) {
        Options = options;
        Logger = logger;
        MemoryStreamManager = memoryStreamManager;
    }
}
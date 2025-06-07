using Caxivitual.Lunacub.Building.Collections;
using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options.
/// </summary>
public sealed class SerializationContext {
    /// <summary>
    /// User-provided options that can be used during the processing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    /// <summary>
    /// Gets the <see cref="ILogger"/> instance used for debugging and reporting.
    /// </summary>
    public ILogger Logger { get; }

    internal SerializationContext(IImportOptions? options, ILogger logger) {
        Options = options;
        Logger = logger;
    }
}
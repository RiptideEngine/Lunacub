using Caxivitual.Lunacub.Building.Collections;

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

    internal SerializationContext(IImportOptions? options) {
        Options = options;
    }
}
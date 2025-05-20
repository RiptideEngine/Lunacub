using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the processing process, providing access to import options.
/// </summary>
public sealed class ProcessingContext {
    /// <summary>
    /// User-provided options that can be used during the processing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }

    internal ProcessingContext(IImportOptions? options) {
        Options = options;
    }
}
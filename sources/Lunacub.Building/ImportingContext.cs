using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options and mechanisms to
/// request building references.
/// </summary>
public sealed class ImportingContext {
    /// <summary>
    /// User-provided options that can be used during the importing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    internal ImportingContext(IImportOptions? options) {
        Options = options;
    }
}
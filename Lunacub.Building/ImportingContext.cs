using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options and mechanisms to
/// request building references.
/// </summary>
public sealed class ImportingContext {
    private readonly HashSet<ResourceID> _references;
    
    // TODO: Move References to processor?
    internal IReadOnlyCollection<ResourceID> References => _references;
    
    /// <summary>
    /// User-provided options that can be used during the importing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    internal ImportingContext(IImportOptions? options) {
        _references = [];
        
        Options = options;
    }

    /// <summary>
    /// Request a reference resource to be build with.
    /// </summary>
    /// <param name="rid">Id of the reference resource to be registered.</param>
    public void AddReference(ResourceID rid) {
        _references.Add(rid);
    }

    /// <summary>
    /// Unregister a reference resource to be build with.
    /// </summary>
    /// <param name="rid">Id of the reference resource to be unregistered.</param>
    public bool RemoveReference(ResourceID rid) {
        return _references.Remove(rid);
    }
}
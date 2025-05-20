using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the importing process, providing access to import options and mechanisms to
/// request building references.
/// </summary>
public sealed class ImportingContext {
    internal ICollection<ResourceID> References { get; }
    
    /// <summary>
    /// User-provided options that can be used during the importing process.
    /// </summary>
    /// <seealso cref="BuildingResource.Options"/>
    public IImportOptions? Options { get; }
    
    internal ImportingContext(IImportOptions? options) {
        References = new List<ResourceID>();    // A HashSet work too but I'm trying to keep things slim here.
        Options = options;
    }

    /// <summary>
    /// Request a reference resource to be build with.
    /// </summary>
    /// <param name="rid">Id of the reference resource to be registered.</param>
    public void AddReference(ResourceID rid) {
        if (References.Contains(rid)) return;
        
        References.Add(rid);
    }

    /// <summary>
    /// Unregister a reference resource to be build with.
    /// </summary>
    /// <param name="rid">Id of the reference resource to be unregistered.</param>
    public bool RemoveReference(ResourceID rid) {
        return References.Remove(rid);
    }
}
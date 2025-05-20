namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the access to informations of a single resource.
/// </summary>
public abstract class ResourceProvider {
    /// <summary>
    /// Gets the last write time of resource.
    /// </summary>
    public abstract DateTime LastWriteTime { get; }
    
    /// <summary>
    /// Gets the readable stream of resource data to be imported by <see cref="Importer"/>.
    /// </summary>
    /// <returns>A readable stream of </returns>
    public abstract Stream GetStream();
}
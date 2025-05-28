namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the status of a singular resource build process.
/// </summary>
public enum BuildStatus {
    /// <summary>
    /// Indicates that the resource build process completed successfully without any errors.
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Indicates that the resource doesn't need to be rebuilt because it's already up-to-date.
    /// </summary>
    Cached = 1,

    /// <summary>
    /// Indicates that the resource is not registered.
    /// </summary>
    /// <seealso cref="BuildEnvironment.Resources"/>
    ResourceNotFound = -1,
    
    /// <summary>
    /// Indicates that the resource is requesting to be imported by an unregistered <see cref="Importer"/>.
    /// </summary>
    /// <seealso cref="BuildEnvironment.Importers"/>
    UnknownImporter = -2,
    
    /// <summary>
    /// Indicates that the resource failed to be imported by <see cref="Importer"/> due to an exception.
    /// </summary>
    ImportingFailed = -3,
    
    /// <summary>
    /// Indicates that the resource is requesting to be processed by an unregistered <see cref="Processor"/>.
    /// </summary>
    /// <seealso cref="BuildEnvironment.Processors"/>
    UnknownProcessor = -4,
    
    /// <summary>
    /// Indicates that the resource's requested <see cref="Processor"/> cannot process the object returned by the importer.
    /// </summary>
    /// <seealso cref="Processor.CanProcess"/>
    CannotProcess = -5,
    
    /// <summary>
    /// Indicates that the resource failed to be processed by <see cref="Processor"/> due to an exception.
    /// </summary>
    ProcessingFailed = -6,
    
    /// <summary>
    /// Indicates that the resource provider returned an invalid <see cref="Stream"/> instance.
    /// </summary>
    /// <see cref="ResourceProvider.GetStream"/>
    InvalidResourceStream = -7,
    
    /// <summary>
    /// Indicates that the resource failed to be compiled and output due to an exception.
    /// </summary>
    CompilationFailed = -8,
}
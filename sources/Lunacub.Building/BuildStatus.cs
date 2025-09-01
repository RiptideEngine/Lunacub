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
    /// Indicates that the resource is requesting to be imported by an unregistered <see cref="Importer"/>.
    /// </summary>
    /// <seealso cref="BuildEnvironment.Importers"/>
    UnknownImporter = 1000,
    
    /// <summary>
    /// Indicates that the <see cref="BuildingResource"/> instance is deemed invalid by the <see cref="Importer"/>.
    /// </summary>
    InvalidBuildingResource,
    
    /// <summary>
    /// Indicates that the <see cref="BuildResourceLibrary"/> failed to collect <see cref="Stream"/> instances of a <see cref="BuildingResource"/>.
    /// </summary>
    GetSourceStreamsFail,
    
    /// <summary>
    /// Indicates that the library returned a <see cref="Stream"/> instance that is either writable, or not seekable nor seekable.
    /// </summary>
    /// <remarks>A writable <see cref="Stream"/> is not allowed for security reason.</remarks>
    InvalidResourceStream,
    
    /// <summary>
    /// Indicates that the <see cref="BuildResourceLibrary"/> returned a null primary <see cref="Stream"/> of the resource content.
    /// </summary>
    NullPrimaryResourceStream,
    
    /// <summary>
    /// Indicates that the <see cref="BuildResourceLibrary"/> returned a null secondary <see cref="Stream"/> of the resource content.
    /// </summary>
    NullSecondaryResourceStream,
    
    /// <summary>
    /// Indicates that the <see cref="Importer"/> failed to extract the dependency informations from the resource stream.
    /// </summary>
    ExtractDependenciesFailed,
    
    /// <summary>
    /// Indicates that the <see cref="BuildResourceLibrary"/> failed to retrieve the last write time information.
    /// </summary>
    GetSourceLastWriteTimesFailed,
    
    /// <summary>
    /// Indicates that the resource failed to be imported by <see cref="Importer"/> due to an exception.
    /// </summary>
    ImportingFailed,
    
    /// <summary>
    /// Indicates that the resource is requesting to be processed by an unregistered <see cref="Processor"/>.
    /// </summary>
    /// <seealso cref="BuildEnvironment.Processors"/>
    UnknownProcessor = 2000,
    
    /// <summary>
    /// Indicates that the <see cref="Processor"/> cannot process the <see cref="object"/> returned by
    /// <see cref="Importer"/>.
    /// </summary>
    Unprocessable,
    
    /// <summary>
    /// Indicates that the resource failed to be processed by <see cref="Processor"/> due to an exception.
    /// </summary>
    ProcessingFailed,
    
    /// <summary>
    /// Indicates that the resource failed to be serialized due to an exception.
    /// </summary>
    SerializationFailed = 3000,
}
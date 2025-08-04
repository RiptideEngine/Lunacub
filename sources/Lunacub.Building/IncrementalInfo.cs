using Caxivitual.Lunacub.Building.Serialization;
using Caxivitual.Lunacub.Building.Collections;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// A structure contains the informations needed to know whether a resource has been modified or not and can be used by
/// <see cref="BuildEnvironment"/> to determine whether or not a resource need to be rebuilt.
/// </summary>
[JsonConverter(typeof(IncrementalInfoConverter))]
[ExcludeFromCodeCoverage]
public readonly struct IncrementalInfo {
    /// <summary>
    /// Represents the last write time of the source resource data.
    /// </summary>
    public readonly SourceLastWriteTimes SourcesLastWriteTime;
    
    /// <summary>
    /// Represents the options from the last build of the resource.
    /// </summary>
    /// <see cref="BuildingResource.Options"/>
    public readonly BuildingOptions Options;
    
    /// <summary>
    /// A set of <see cref="ResourceID"/> that contains the resources that the resource depends on.
    /// </summary>
    public readonly IReadOnlySet<ResourceAddress> DependencyAddresses;

    /// <summary>
    /// Contains the versioning strings of building components.
    /// </summary>
    public readonly ComponentVersions ComponentVersions;

    /// <summary>
    /// Initializes a new instance of <see cref="IncrementalInfo"/> with empty dependencies, specified source resource
    /// last write time and options.
    /// </summary>
    /// <param name="sourcesLastWriteTime">The last write times of resource sources.</param>
    /// <param name="options">The previously build options of the resource.</param>
    /// <param name="componentVersions">
    ///     The version strings of <see cref="Importer"/> and <see cref="Processor"/> that used to import and process the resource.
    /// </param>
    internal IncrementalInfo(SourceLastWriteTimes sourcesLastWriteTime, BuildingOptions options, ComponentVersions componentVersions) :
        this(sourcesLastWriteTime, options, FrozenSet<ResourceAddress>.Empty, componentVersions) {}
    
    /// <summary>
    /// Initializes a new instance of <see cref="IncrementalInfo"/> with a specified source resource last write time, options,
    /// dependencies and dependents.
    /// </summary>
    /// <param name="sourcesLastWriteTime">The last write times of resource sources.</param>
    /// <param name="options">The previously build options of the resource.</param>
    /// <param name="dependencyAddresses">
    /// A set of <see cref="ResourceAddress"/> that represents all the resources that the owner resource depends on.
    /// </param>
    /// <param name="componentVersions">
    ///     The version strings of <see cref="Importer"/> and <see cref="Processor"/> that used to import and process the resource
    /// </param>
    internal IncrementalInfo(
        SourceLastWriteTimes sourcesLastWriteTime,
        BuildingOptions options,
        IReadOnlySet<ResourceAddress> dependencyAddresses,
        ComponentVersions componentVersions
    ) {
        SourcesLastWriteTime = sourcesLastWriteTime;
        Options = options;
        DependencyAddresses = dependencyAddresses;
        ComponentVersions = componentVersions;
    }
}
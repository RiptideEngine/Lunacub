using Caxivitual.Lunacub.Building.Serialization;
using Caxivitual.Lunacub.Building.Collections;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// A structure contains the informations needed to know whether a resource has been modified or not and can be used by
/// <see cref="BuildEnvironment"/> to determine whether or not a resource need to be rebuilt.
/// </summary>
[JsonConverter(typeof(IncrementalInfoConverter))]
public readonly struct IncrementalInfo {
    /// <summary>
    /// Represents the last write time of the source resource data.
    /// </summary>
    public readonly DateTime SourceLastWriteTime;
    
    /// <summary>
    /// Represents the options from the last build of the resource.
    /// </summary>
    /// <see cref="BuildingResource.Options"/>
    public readonly BuildingOptions Options;
    
    /// <summary>
    /// A set of <see cref="ResourceID"/> that contains the resources that the resource depends on.
    /// </summary>
    public readonly IReadOnlySet<ResourceID> Dependencies;

    /// <summary>
    /// Initializes a new instance of <see cref="IncrementalInfo"/> with empty dependencies, specified source resource
    /// last write time and options.
    /// </summary>
    /// <param name="sourceLastWriteTime">The last write time of the source resource data.</param>
    /// <param name="options">The previously build options of the resource.</param>
    public IncrementalInfo(DateTime sourceLastWriteTime, BuildingOptions options) : this(sourceLastWriteTime, options, FrozenSet<ResourceID>.Empty) {}
    
    /// <summary>
    /// Initializes a new instance of <see cref="IncrementalInfo"/> with a specified source resource last write time, options,
    /// dependencies and dependents.
    /// </summary>
    /// <param name="sourceLastWriteTime">The last write time of the source resource data.</param>
    /// <param name="options">The previously build options of the resource.</param>
    /// <param name="dependencies">A set of <see cref="ResourceID"/> that contains the resources that the resource depends on.</param>
    public IncrementalInfo(DateTime sourceLastWriteTime, BuildingOptions options, IReadOnlySet<ResourceID> dependencies) {
        SourceLastWriteTime = sourceLastWriteTime;
        Options = options;
        Dependencies = dependencies;
    }
}
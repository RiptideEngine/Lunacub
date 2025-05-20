using Caxivitual.Lunacub.Building.Serialization;
using Caxivitual.Lunacub.Building.Collections;

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
    /// Initializes a new instance of <see cref="IncrementalInfo"/> with a specified source resource last write time
    /// and options.
    /// </summary>
    /// <param name="sourceLastWriteTime">The last write time of the source resource data.</param>
    /// <param name="options">The previously build options of the resource.</param>
    public IncrementalInfo(DateTime sourceLastWriteTime, BuildingOptions options) {
        SourceLastWriteTime = sourceLastWriteTime;
        Options = options;
    }
}
using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the result of an overall resource building process.
/// </summary>
public readonly struct BuildingResult {
    /// <summary>
    /// The <see cref="DateTime"/> when resource building begin.
    /// </summary>
    public readonly DateTime BuildStartTime;
    
    /// <summary>
    /// The <see cref="DateTime"/> when resource building finish.
    /// </summary>
    public readonly DateTime BuildFinishTime;
    
    /// <summary>
    /// The collections of every resources involves in the building process and their building results.
    /// </summary>
    public readonly EnvironmentLibraryDictionary<ResourceResultDictionary> EnvironmentResults;

    internal BuildingResult(
        DateTime startTime,
        DateTime finishTime,
        EnvironmentLibraryDictionary<ResourceResultDictionary> environmentResults
    ) {
        BuildStartTime = startTime;
        BuildFinishTime = finishTime;
        EnvironmentResults = environmentResults;
    }
}
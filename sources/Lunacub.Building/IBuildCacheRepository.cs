namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents a data entrypoint for resource incremental information.
/// </summary>
public interface IBuildCacheRepository {
    /// <summary>
    /// Collects all the previously created <see cref="BuildCache"/> into a container.
    /// </summary>
    /// <param name="receiver">The container that receives the <see cref="BuildCache"/> for each library.</param>
    void CollectIncrementalInfos(EnvironmentBuildCache receiver);
}
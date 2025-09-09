namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents an output endpoint for resource incremental information.
/// </summary>
public interface IBuildCacheSink {
    /// <summary>
    /// Flushes all the storing <see cref="BuildCache"/> to a persistent storage to be reused later.
    /// </summary>
    /// <param name="incrementalInfos">
    /// The container that contains all the <see cref="BuildCache"/> generated from building sessions.
    /// </param>
    void FlushBuildCaches(EnvironmentBuildCache buildCache);
}
namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the imported resource disposal process, providing access to logger and other properties.
/// request building references.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct DisposingContext {
    private readonly BuildEnvironment _environment;

    /// <summary>
    /// Gets the logger used for debugging and logging purpose.
    /// </summary>
    public ILogger Logger => _environment.Logger;
    
    /// <summary>
    /// Gets the dictionary of environment variables associates with the build environment.
    /// </summary>
    public IReadOnlyDictionary<object, object> EnvironmentVariables => _environment.EnvironmentVariables;
    
    internal DisposingContext(BuildEnvironment environment) {
        _environment = environment;
    }
}
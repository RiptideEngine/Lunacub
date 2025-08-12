namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the context used during the imported resource disposal process, providing access to logger and other properties.
/// request building references.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct DisposingContext {
    /// <summary>
    /// Gets the logger used for debugging and printing purpose.
    /// </summary>
    public ILogger Logger { get; }
    
    internal DisposingContext(ILogger logger) {
        Logger = logger;
    }
}
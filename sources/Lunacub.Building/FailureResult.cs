namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the failure result of a single resource during building process.
/// </summary>
public readonly struct FailureResult {
    /// <summary>
    /// The resource building status.
    /// </summary>
    public readonly BuildStatus Status;
    
    /// <summary>
    /// The exception got thrown while building resource. Not guarantee to be non-null when <see cref="Status"/> denotes failure.
    /// </summary>
    public readonly ExceptionDispatchInfo? Exception;

    internal FailureResult(BuildStatus status, ExceptionDispatchInfo? exception = null) {
        Status = status;
        Exception = exception;
    }
}
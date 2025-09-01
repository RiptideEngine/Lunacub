namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the result of a single resource building process.
/// </summary>
public readonly struct ResourceBuildingResult {
    /// <summary>
    /// The resource building status.
    /// </summary>
    public readonly BuildStatus Status;
    
    /// <summary>
    /// The exception got thrown while building resource. Not guarantee to be non-null when <see cref="Status"/> denotes failure.
    /// </summary>
    public readonly ExceptionDispatchInfo? Exception;
    
    /// <summary>
    /// Indicates whether the resource building process completed successfully.
    /// </summary>
    public bool IsSuccess => Status is < BuildStatus.NullPrimaryResourceStream and >= BuildStatus.Success;

    internal ResourceBuildingResult(BuildStatus status, ExceptionDispatchInfo? exception = null) {
        Status = status;
        Exception = exception;
    }
}
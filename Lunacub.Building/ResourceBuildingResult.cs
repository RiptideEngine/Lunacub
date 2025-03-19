namespace Caxivitual.Lunacub.Building;

public readonly struct ResourceBuildingResult {
    public readonly BuildStatus Status;
    public readonly Exception? Exception;

    public bool IsSuccess => Status >= BuildStatus.Success;

    internal ResourceBuildingResult(BuildStatus status, Exception? exception = null) {
        Status = status;
        Exception = exception;
    }
}
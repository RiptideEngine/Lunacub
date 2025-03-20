using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

public readonly struct ResourceBuildingResult {
    public readonly BuildStatus Status;
    public readonly ExceptionDispatchInfo? Exception;

    public bool IsSuccess => Status >= BuildStatus.Success;

    internal ResourceBuildingResult(BuildStatus status, ExceptionDispatchInfo? exception = null) {
        Status = status;
        Exception = exception;
    }
}
namespace Caxivitual.Lunacub.Building;

public enum BuildStatus {
    Success = 0,
    Cached = 1,
    
    ResourceNotFound = -1,
    UnknownImporter = -2,
    ImportingFailed = -3,
    UnknownProcessor = -4,
    CannotProcess = -5,
    ProcessingFailed = -6,
    CompilationFailed = -7,
}
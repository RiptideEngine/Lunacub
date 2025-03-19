namespace Caxivitual.Lunacub.Building;

public enum BuildStatus {
    Success = 0,
    Cached = 1,
    
    ResourceNotFound = -1,
    UnknownImporter = -2,
    UnknownProcessor = -3,
}
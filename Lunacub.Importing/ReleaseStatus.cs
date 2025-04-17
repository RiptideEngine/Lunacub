namespace Caxivitual.Lunacub.Importing;

public enum ReleaseStatus {
    Success = 0,
    NotDisposed,
    Null,
    
    ResourceNotImported = -1,
    ResourceIncompatible = -2,
    Unspecified = int.MinValue,
}
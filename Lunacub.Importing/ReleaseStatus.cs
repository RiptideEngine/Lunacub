namespace Caxivitual.Lunacub.Importing;

public enum ReleaseStatus {
    Success = 0,
    Canceled,
    NotDisposed,
    Null,
    
    InvalidResource = -1,
    ResourceIncompatible = -2,
    UnregisteredResourceID = -3,
    Unspecified = int.MinValue,
}
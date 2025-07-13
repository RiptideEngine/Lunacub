namespace Caxivitual.Lunacub.Importing;

public enum ImportingStatus : byte {
    Importing,
    Success,
    Failed,
    Cancelled,
    Disposed,
}
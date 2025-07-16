namespace Caxivitual.Lunacub.Importing;

public enum ImportingStatus : byte {
    Importing,
    Success,
    Failed,
    Canceled,
    Disposed,
}
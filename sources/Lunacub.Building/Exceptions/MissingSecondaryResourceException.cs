namespace Caxivitual.Lunacub.Building.Exceptions;

public sealed class MissingSecondaryResourceException : InvalidOperationException {
    public MissingSecondaryResourceException(string message) : base(message) { }
    public MissingSecondaryResourceException(string message, Exception? innerException) : base(message, innerException) { }
}
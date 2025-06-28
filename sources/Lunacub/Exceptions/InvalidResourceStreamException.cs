namespace Caxivitual.Lunacub.Exceptions;

public class InvalidResourceStreamException : InvalidOperationException {
    public InvalidResourceStreamException(string message) : base(message) { }
    public InvalidResourceStreamException(string message, Exception innerException) : base(message, innerException) { }
}
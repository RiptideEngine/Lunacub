namespace Caxivitual.Lunacub.Exceptions;

[ExcludeFromCodeCoverage]
public class InvalidResourceStreamException : InvalidOperationException {
    public InvalidResourceStreamException(string message) : base(message) { }
    public InvalidResourceStreamException(string message, Exception innerException) : base(message, innerException) { }
}
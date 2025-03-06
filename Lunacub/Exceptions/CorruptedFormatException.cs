namespace Caxivitual.Lunacub.Exceptions;

public class CorruptedFormatException : FormatException {
    public CorruptedFormatException(string message) : base(message) { }
    public CorruptedFormatException(string message, Exception innerException) : base(message, innerException) { }
}
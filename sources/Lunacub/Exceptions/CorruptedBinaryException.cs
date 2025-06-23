namespace Caxivitual.Lunacub.Exceptions;
 
[ExcludeFromCodeCoverage]
public class CorruptedBinaryException : FormatException {
    public CorruptedBinaryException(string message) : base(message) { }
    public CorruptedBinaryException(string message, Exception innerException) : base(message, innerException) { }
}
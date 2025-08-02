namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ExceptionThrowingProcessor<T> : Processor<T, T> where T : ContentRepresentation {
    protected override T Process(T importedObject, ProcessingContext context) {
        throw new("This exception is expected.");
    }
}
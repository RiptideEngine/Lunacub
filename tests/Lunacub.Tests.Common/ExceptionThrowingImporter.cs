namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ExceptionThrowingImporter<T> : Importer<T> where T : ContentRepresentation {
    protected override T Import(SourceStreams sourceStreams, ImportingContext context) {
        throw new("This exception is expected.");
    }
}
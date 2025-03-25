namespace Caxivitual.Lunacub.Building;

public abstract class Importer {
    internal abstract ContentRepresentation ImportObject(Stream stream, ImportingContext context);
}

public abstract class Importer<T> : Importer where T : ContentRepresentation {
    internal override sealed ContentRepresentation ImportObject(Stream stream, ImportingContext context) {
        return Import(stream, context);
    }

    protected abstract T Import(Stream stream, ImportingContext context);
    protected virtual void Dispose(T obj) { }
}
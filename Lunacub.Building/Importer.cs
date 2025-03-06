namespace Caxivitual.Lunacub.Building;

public abstract class Importer {
    internal abstract ContentRepresentation ImportObject(Stream stream);
    internal virtual void DisposeObject(ContentRepresentation obj) { }
}

public abstract class Importer<T> : Importer where T : ContentRepresentation {
    internal override sealed ContentRepresentation ImportObject(Stream stream) {
        return Import(stream);
    }

    internal override sealed void DisposeObject(ContentRepresentation obj) {
        Dispose((T)obj);
    }
    
    protected abstract T Import(Stream stream);
    protected virtual void Dispose(T obj) { }
}
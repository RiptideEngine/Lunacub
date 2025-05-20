namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the importing process of a resource.
/// </summary>
public abstract class Importer {
    internal abstract ContentRepresentation ImportObject(Stream stream, ImportingContext context);
}

/// <inheritdoc cref="Importer"/>
/// <typeparam name="T">The type of object that the importer will output, must derived from <see cref="ContentRepresentation"/>.</typeparam>
public abstract class Importer<T> : Importer where T : ContentRepresentation {
    internal override sealed ContentRepresentation ImportObject(Stream stream, ImportingContext context) {
        return Import(stream, context);
    }

    protected abstract T Import(Stream stream, ImportingContext context);
}
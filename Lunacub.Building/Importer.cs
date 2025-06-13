using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the importing process of a resource.
/// </summary>
public abstract class Importer {
    public virtual string? Version => null;

    public virtual IReadOnlyCollection<ResourceID> ExtractDependencies(Stream resourceStream) {
        return FrozenSet<ResourceID>.Empty;
    }
    
    internal abstract ContentRepresentation ImportObject(Stream resourceStream, ImportingContext context);
}

/// <inheritdoc cref="Importer"/>
/// <typeparam name="T">The type of object that the importer will output, must derived from <see cref="ContentRepresentation"/>.</typeparam>
public abstract class Importer<T> : Importer where T : ContentRepresentation {
    internal override sealed ContentRepresentation ImportObject(Stream resourceStream, ImportingContext context) {
        return Import(resourceStream, context);
    }

    protected abstract T Import(Stream resourceStream, ImportingContext context);
}
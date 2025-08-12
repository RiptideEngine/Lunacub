namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the importing process of a resource.
/// </summary>
public abstract class Importer {
    public virtual string? Version => null;
    public virtual ImporterFlags Flags => ImporterFlags.Default;

    public virtual void ValidateResource(BuildingResource resource) { }

    public virtual IReadOnlyCollection<ResourceAddress> ExtractDependencies(SourceStreams sourceStream) {
        return FrozenSet<ResourceAddress>.Empty;
    }
    
    internal abstract object ImportObject(SourceStreams sourceStreams, ImportingContext context);
    internal virtual void Dispose(object obj, DisposingContext context) { }
}

/// <inheritdoc cref="Importer"/>
/// <typeparam name="T">The type of object that the importer will output.</typeparam>
public abstract class Importer<T> : Importer {
    internal override sealed object ImportObject(SourceStreams sourceStreams, ImportingContext context) {
        return Import(sourceStreams, context)!;
    }

    internal override void Dispose(object obj, DisposingContext context) {
        Dispose((T)obj, context);
    }

    protected abstract T Import(SourceStreams sourceStreams, ImportingContext context);

    protected virtual void Dispose(T obj, DisposingContext context) { }
}
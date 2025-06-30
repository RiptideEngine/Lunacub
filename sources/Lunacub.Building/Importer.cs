using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the importing process of a resource.
/// </summary>
public abstract class Importer {
    public virtual string? Version => null;
    public virtual ImporterFlags Flags => ImporterFlags.Default;

    public virtual void ValidateResource(BuildingResource resource) { }

    public virtual IReadOnlySet<ResourceID> ExtractDependencies(SourceStreams sourceStream) {
        return FrozenSet<ResourceID>.Empty;
    }
    
    internal abstract ContentRepresentation ImportObject(SourceStreams sourceStreams, ImportingContext context);
}

/// <inheritdoc cref="Importer"/>
/// <typeparam name="T">The type of object that the importer will output, must derived from <see cref="ContentRepresentation"/>.</typeparam>
public abstract class Importer<T> : Importer where T : ContentRepresentation {
    internal override sealed ContentRepresentation ImportObject(SourceStreams sourceStreams, ImportingContext context) {
        return Import(sourceStreams, context);
    }

    protected abstract T Import(SourceStreams sourceStreams, ImportingContext context);
}
﻿using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct BuildingProceduralResource : IEquatable<BuildingProceduralResource> {
    /// <summary>
    /// Gets the building resource as an instance of <see cref="ContentRepresentation"/>.
    /// </summary>
    public required ContentRepresentation Object { get; init; }
    
    /// <summary>
    /// Gets the dependency ids of the resource.
    /// </summary>
    public IReadOnlySet<ResourceID> DependencyIds { get; init; }
    
    /// <summary>
    /// Gets the name of <see cref="Processor"/> used to convert the <see cref="ContentRepresentation"/> after the
    /// importing stage into another <see cref="ContentRepresentation"/>.
    /// </summary>
    public string? ProcessorName { get; init; }
    
    /// <summary>
    /// Get the tags of the resource.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; }
    
    /// <summary>
    /// Gets the importing option object that can be used for building resource.
    /// </summary>
    public IImportOptions? Options { get; init; }

    public BuildingProceduralResource() {
        Tags = [];
        Options = null;
        DependencyIds = FrozenSet<ResourceID>.Empty;
    }

    [SetsRequiredMembers]
    public BuildingProceduralResource(ContentRepresentation obj, string? processorName) :
        this(obj, processorName, [], null) { }

    [SetsRequiredMembers]
    public BuildingProceduralResource(
        ContentRepresentation obj,
        string? processorName,
        IReadOnlyCollection<string> tags,
        IImportOptions? options
    ) {
        Object = obj;
        DependencyIds = FrozenSet<ResourceID>.Empty;
        ProcessorName = processorName;
        Tags = tags;
        Options = options;
    }
    
    public bool Equals(BuildingProceduralResource other) {
        return Object == other.Object &&
               DependencyIds.SequenceEqual(other.DependencyIds) &&
               ProcessorName == other.ProcessorName &&
               Tags.SequenceEqual(other.Tags) &&
               (Options?.Equals(other.Options) ?? other.Options == null);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingProceduralResource other && Equals(other);
    
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() {
        HashCode hc = new();
        
        hc.Add(Object);
        hc.Add(DependencyIds);
        hc.Add(ProcessorName);
        hc.Add(Options);
        
        foreach (var tag in Tags) {
            hc.Add(tag);
        }
        
        return hc.ToHashCode();
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(nameof(BuildingOptions));
        sb.Append(" { ");
        sb.Append(nameof(Object)).Append('=').Append(Object).Append(", ");
        sb.Append(nameof(ProcessorName)).Append('=').Append(ProcessorName).Append(", ");
        sb.Append(nameof(Tags)).Append("=[").AppendJoin(", ", Tags).Append("], ");
        sb.Append(nameof(Options)).Append(" = ").Append(Options);
        sb.Append(" }");
        
        return sb.ToString();
    }

    public static bool operator ==(BuildingProceduralResource left, BuildingProceduralResource right) {
        return left.Equals(right);
    }

    public static bool operator !=(BuildingProceduralResource left, BuildingProceduralResource right) {
        return !(left == right);
    }
}
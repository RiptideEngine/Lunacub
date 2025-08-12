namespace Caxivitual.Lunacub.Building;

[DebuggerDisplay("{ToString(),nq}")]
public readonly struct BuildingProceduralResource : IEquatable<BuildingProceduralResource> {
    /// <summary>
    /// Gets the building resource as an instance of <see cref="object"/>.
    /// </summary>
    public required object Object { get; init; }
    
    /// <summary>
    /// Gets the dependency ids of the resource.
    /// </summary>
    public IReadOnlySet<ResourceAddress> DependencyAddresses { get; init; }
    
    /// <summary>
    /// Gets the name of <see cref="Processor"/> used to convert the <see cref="object"/> after the
    /// importing stage into another <see cref="object"/>.
    /// </summary>
    public string? ProcessorName { get; init; }
    
    /// <summary>
    /// Get the tags of the resource.
    /// </summary>
    public ImmutableArray<string> Tags { get; init; }
    
    /// <summary>
    /// Gets the importing option object that can be used for building resource.
    /// </summary>
    public IImportOptions? Options { get; init; }

    public BuildingProceduralResource() {
        Tags = [];
        Options = null;
        DependencyAddresses = FrozenSet<ResourceAddress>.Empty;
    }

    [SetsRequiredMembers]
    public BuildingProceduralResource(object obj, string? processorName) :
        this(obj, processorName, [], null) { }

    [SetsRequiredMembers]
    public BuildingProceduralResource(
        object obj,
        string? processorName,
        ImmutableArray<string> tags,
        IImportOptions? options
    ) {
        Object = obj;
        DependencyAddresses = FrozenSet<ResourceAddress>.Empty;
        ProcessorName = processorName;
        Tags = tags;
        Options = options;
    }
    
    public bool Equals(BuildingProceduralResource other) {
        return Object == other.Object &&
               DependencyAddresses.SequenceEqual(other.DependencyAddresses) &&
               ProcessorName == other.ProcessorName &&
               Tags.SequenceEqual(other.Tags) &&
               (Options?.Equals(other.Options) ?? other.Options == null);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingProceduralResource other && Equals(other);
    
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() {
        HashCode hc = new();
        
        hc.Add(Object);
        hc.Add(DependencyAddresses);
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
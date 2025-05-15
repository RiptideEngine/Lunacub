using Caxivitual.Lunacub.Building.Serialization;

namespace Caxivitual.Lunacub.Building;

[JsonConverter(typeof(BuildingOptionsConverter))]
public readonly struct BuildingOptions : IEquatable<BuildingOptions> {
    public required string ImporterName { get; init; }
    public string? ProcessorName { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; }
    public IImportOptions? Options { get; init; }

    [ExcludeFromCodeCoverage]
    public BuildingOptions() {
        ProcessorName = string.Empty;
        Tags = [];
    }
    
    [SetsRequiredMembers]
    public BuildingOptions(string importerName, string? processorName = null) : this(importerName, processorName, [], null) { }
    
    [SetsRequiredMembers]
    public BuildingOptions(string importerName, string? processorName, IReadOnlyCollection<string>? tags, IImportOptions? options) {
        ImporterName = importerName;
        ProcessorName = processorName;
        Tags = tags ?? [];
        Options = options;
    }

    public bool Equals(BuildingOptions other) {
        return ImporterName == other.ImporterName && ProcessorName == other.ProcessorName &&
               Tags.SequenceEqual(other.Tags) &&
               (Options?.Equals(other.Options) ?? other.Options == null);
    }

    [ExcludeFromCodeCoverage]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingOptions other && Equals(other);
    
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() {
        HashCode hc = new();
        
        hc.Add(ImporterName);
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
        sb.Append(nameof(ImporterName)).Append('=').Append(ImporterName).Append(", ");
        sb.Append(nameof(ProcessorName)).Append('=').Append(ProcessorName).Append(", ");
        sb.Append(nameof(Tags)).Append("=[").AppendJoin(", ", Tags).Append("], ");
        sb.Append(nameof(Options)).Append(" = ").Append(Options);
        sb.Append(" }");
        
        return sb.ToString();
    }

    [ExcludeFromCodeCoverage] public static bool operator ==(BuildingOptions left, BuildingOptions right) => left.Equals(right);
    [ExcludeFromCodeCoverage] public static bool operator !=(BuildingOptions left, BuildingOptions right) => !(left == right);
}
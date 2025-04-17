using Caxivitual.Lunacub.Building.Serialization;

namespace Caxivitual.Lunacub.Building;

[JsonConverter(typeof(BuildingOptionsConverter))]
public readonly struct BuildingOptions : IEquatable<BuildingOptions> {
    public readonly string ImporterName;
    public readonly string ProcessorName;
    public readonly IReadOnlyCollection<string> Tags;
    public readonly IImportOptions? Options;

    public BuildingOptions(string importerName, string? processorName = null) : this(importerName, processorName, Array.Empty<string>(), null) { }
    public BuildingOptions(string importerName, string? processorName, IReadOnlyCollection<string>? tags, IImportOptions? options) {
        ImporterName = importerName;
        ProcessorName = processorName ?? string.Empty;
        Tags = tags ?? Array.Empty<string>();
        Options = options;
    }

    public bool Equals(BuildingOptions other) {
        if (ImporterName != other.ImporterName || ProcessorName != other.ProcessorName) return false;
        if (!Tags.SequenceEqual(other.Tags)) return false;
        if (Options == null) return other.Options == null;
        
        return Options.Equals(other.Options);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingOptions other && Equals(other);
    
    public override int GetHashCode() {
        HashCode hc = new();
        
        hc.Add(ImporterName);
        hc.Add(ProcessorName);
        hc.Add(Options);
        
        hc.Add(Tags.Count);
        foreach (var tag in Tags) {
            hc.Add(tag);
        }
        
        return hc.ToHashCode();
    }

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

    public static bool operator ==(BuildingOptions left, BuildingOptions right) => left.Equals(right);
    public static bool operator !=(BuildingOptions left, BuildingOptions right) => !(left == right);
}
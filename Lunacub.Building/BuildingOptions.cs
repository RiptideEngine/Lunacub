namespace Caxivitual.Lunacub.Building;

public readonly struct BuildingOptions : IEquatable<BuildingOptions> {
    public readonly string ImporterName;
    public readonly string? ProcessorName;
    private readonly IReadOnlyCollection<string> Tags;
    public readonly IImportOptions? Options;

    public BuildingOptions(string importerName, string? processorName = null) : this(importerName, processorName, Array.Empty<string>(), null) { }
    public BuildingOptions(string importerName, string? processorName, IReadOnlyCollection<string> tags, IImportOptions? options) {
        ImporterName = importerName;
        ProcessorName = processorName;
        Tags = tags;
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

    public static bool operator ==(BuildingOptions left, BuildingOptions right) => left.Equals(right);
    public static bool operator !=(BuildingOptions left, BuildingOptions right) => !(left == right);
}
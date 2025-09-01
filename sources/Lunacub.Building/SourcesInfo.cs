namespace Caxivitual.Lunacub.Building;

public readonly struct SourcesInfo {
    public SourceInfo Primary { get; }
    public IReadOnlyDictionary<string, SourceInfo>? Secondaries { get; }
    
    public SourcesInfo(SourceInfo primary) : this(primary, FrozenDictionary<string, SourceInfo>.Empty) {
    }

    [JsonConstructor]
    public SourcesInfo(SourceInfo primary, IReadOnlyDictionary<string, SourceInfo>? secondaries) {
        Primary = primary;
        Secondaries = secondaries ?? FrozenDictionary<string, SourceInfo>.Empty;
    }
}
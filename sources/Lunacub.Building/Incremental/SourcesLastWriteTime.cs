namespace Caxivitual.Lunacub.Building.Incremental;

public readonly struct SourcesLastWriteTime {
    public DateTime Primary { get; }
    public IReadOnlyDictionary<string, DateTime>? Secondaries { get; }
    
    public SourcesLastWriteTime(DateTime primary) : this(primary, FrozenDictionary<string, DateTime>.Empty) {
    }

    [JsonConstructor]
    public SourcesLastWriteTime(DateTime primary, IReadOnlyDictionary<string, DateTime>? secondaries) {
        Primary = primary;
        Secondaries = secondaries ?? FrozenDictionary<string, DateTime>.Empty;
    }
}
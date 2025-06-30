using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public readonly struct SourceLastWriteTimes : IEquatable<SourceLastWriteTimes> {
    public readonly DateTime Primary;
    public readonly IReadOnlyDictionary<string, DateTime> Secondaries;

    public SourceLastWriteTimes(DateTime primary) : this(primary, FrozenDictionary<string, DateTime>.Empty) {
    }

    public SourceLastWriteTimes(DateTime primary, IReadOnlyDictionary<string, DateTime> secondaries) {
        Primary = primary;
        Secondaries = secondaries;
    }

    public bool Equals(SourceLastWriteTimes other) {
        if (Primary != other.Primary || Secondaries.Count != other.Secondaries.Count) return false;
        if (ReferenceEquals(Secondaries, other.Secondaries)) return true;
        
        foreach ((var key, var value) in Secondaries) {
            if (!other.Secondaries.TryGetValue(key, out var value2)) return false;
            if (value != value2) return false;
        }

        return false;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SourceLastWriteTimes other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Primary, Secondaries);
    
    public static bool operator ==(SourceLastWriteTimes left, SourceLastWriteTimes right) => left.Equals(right);
    public static bool operator !=(SourceLastWriteTimes left, SourceLastWriteTimes right) => !left.Equals(right);
}
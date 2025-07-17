using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public readonly struct SourceAddresses : IEquatable<SourceAddresses> {
    public readonly string Primary;
    public readonly IReadOnlyDictionary<string, string> Secondaries;

    public SourceAddresses(string primary) : this(primary, FrozenDictionary<string, string>.Empty) {
    }

    public SourceAddresses(string primary, IReadOnlyDictionary<string, string> secondaries) {
        Primary = primary;
        Secondaries = secondaries;
    }

    public bool Equals(SourceAddresses other) {
        if (Primary != other.Primary || Secondaries.Count != other.Secondaries.Count) return false;
        if (ReferenceEquals(Secondaries, other.Secondaries)) return true;
        
        foreach ((var key, var value) in Secondaries) {
            if (!other.Secondaries.TryGetValue(key, out var value2)) return false;
            if (value != value2) return false;
        }

        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SourceAddresses other && Equals(other);
    
    [ExcludeFromCodeCoverage] public override int GetHashCode() => HashCode.Combine(Primary, Secondaries);
    
    public static bool operator ==(SourceAddresses left, SourceAddresses right) => left.Equals(right);
    public static bool operator !=(SourceAddresses left, SourceAddresses right) => !left.Equals(right);
}
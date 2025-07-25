﻿using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public readonly struct SourceLastWriteTimes : IEquatable<SourceLastWriteTimes> {
    public DateTime Primary { get; }
    public IReadOnlyDictionary<string, DateTime>? Secondaries { get; }

    public SourceLastWriteTimes(DateTime primary) : this(primary, FrozenDictionary<string, DateTime>.Empty) {
    }

    [JsonConstructor]
    public SourceLastWriteTimes(DateTime primary, IReadOnlyDictionary<string, DateTime>? secondaries) {
        Primary = primary;
        Secondaries = secondaries ?? FrozenDictionary<string, DateTime>.Empty;
    }

    public bool Equals(SourceLastWriteTimes other) {
        if (Primary != other.Primary) return false;
        if (other.Secondaries == null) return Secondaries == null;
        if (Secondaries == null) return false;
        if (ReferenceEquals(other.Secondaries, Secondaries)) return true;

        Debug.Assert(Secondaries != null && other.Secondaries != null);
        
        foreach ((var key, var value) in Secondaries) {
            if (!other.Secondaries.TryGetValue(key, out var value2)) return false;
            if (value != value2) return false;
        }
        
        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SourceLastWriteTimes other && Equals(other);
    
    [ExcludeFromCodeCoverage] public override int GetHashCode() => HashCode.Combine(Primary, Secondaries);
    
    public static bool operator ==(SourceLastWriteTimes left, SourceLastWriteTimes right) => left.Equals(right);
    public static bool operator !=(SourceLastWriteTimes left, SourceLastWriteTimes right) => !left.Equals(right);
}
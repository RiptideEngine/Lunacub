using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public readonly struct ResourceAddresses : IEquatable<ResourceAddresses> {
    public readonly string PrimaryAddress;
    public readonly IReadOnlyDictionary<string, string> SecondaryAddresses;

    public ResourceAddresses(string primaryAddress) : this(primaryAddress, FrozenDictionary<string, string>.Empty) {
    }

    public ResourceAddresses(string primaryAddress, IReadOnlyDictionary<string, string> secondaryAddresses) {
        PrimaryAddress = primaryAddress;
        SecondaryAddresses = secondaryAddresses;
    }

    public bool Equals(ResourceAddresses other) {
        if (PrimaryAddress != other.PrimaryAddress || SecondaryAddresses.Count != other.SecondaryAddresses.Count) return false;
        if (ReferenceEquals(SecondaryAddresses, other.SecondaryAddresses)) return true;
        
        foreach ((var key, var value) in SecondaryAddresses) {
            if (!other.SecondaryAddresses.TryGetValue(key, out var value2)) return false;
            if (value != value2) return false;
        }

        return false;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceAddresses other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(PrimaryAddress, SecondaryAddresses);
    
    public static bool operator ==(ResourceAddresses left, ResourceAddresses right) => left.Equals(right);
    public static bool operator !=(ResourceAddresses left, ResourceAddresses right) => !left.Equals(right);
}
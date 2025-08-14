namespace Caxivitual.Lunacub.Building;

/// <summary>
/// A structure that contains resource address and building options.
/// </summary>
public readonly struct BuildingResource : IEquatable<BuildingResource> {
    /// <summary>
    /// Gets the array of addresses that will be used by <see cref="BuildResourceLibrary"/> to locate resource content.
    /// </summary>
    public required SourceAddresses Addresses { get; init; }
        
    /// <summary>
    /// Gets the resource building options.
    /// </summary>
    public required BuildingOptions Options { get; init; }
        
    [SetsRequiredMembers]
    public BuildingResource(SourceAddresses addresses, BuildingOptions options) {
        Addresses = addresses;
        Options = options;
    }

    public void Deconstruct(out SourceAddresses addresses, out BuildingOptions options) {
        addresses = Addresses;
        options = Options;
    }

    public bool Equals(BuildingResource other) {
        return Addresses == other.Addresses && Options == other.Options;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingResource other && Equals(other);

    [ExcludeFromCodeCoverage] public override int GetHashCode() => HashCode.Combine(Addresses, Options);

    public static bool operator ==(BuildingResource left, BuildingResource right) => left.Equals(right);
    public static bool operator !=(BuildingResource left, BuildingResource right) => !(left == right);
}
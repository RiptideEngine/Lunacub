namespace Caxivitual.Lunacub.Building;

/// <summary>
/// A structure that contains a building options and a <see cref="ResourceProvider"/> instance.
/// </summary>
public readonly struct BuildingResource : IEquatable<BuildingResource> {
    /// <summary>
    /// Gets the resource provider instance.
    /// </summary>
    public required ResourceProvider Provider { get; init; }
        
    /// <summary>
    /// Gets the resource building options.
    /// </summary>
    public required BuildingOptions Options { get; init; }
        
    [SetsRequiredMembers]
    public BuildingResource(ResourceProvider provider, BuildingOptions options) {
        Provider = provider;
        Options = options;
    }

    public void Deconstruct(out ResourceProvider provider, out BuildingOptions options) {
        provider = Provider;
        options = Options;
    }

    public bool Equals(BuildingResource other) {
        return Provider == other.Provider && Options == other.Options;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingResource other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Provider, Options);

    public static bool operator ==(BuildingResource left, BuildingResource right) => left.Equals(right);
    public static bool operator !=(BuildingResource left, BuildingResource right) => !(left == right);
}
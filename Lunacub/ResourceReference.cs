namespace Caxivitual.Lunacub;

public readonly struct ResourceReference(ResourceID rid, string path) : IEquatable<ResourceReference> {
    public ResourceID Rid { get; } = rid;
    public string Path { get; } = path;

    public bool Equals(ResourceReference other) {
        return Rid == other.Rid && Path == other.Path;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceReference other && Equals(other);

    public override int GetHashCode() {
        return HashCode.Combine(Rid, Path);
    }

    public override string ToString() {
        return $"ResourceReference({Rid}, \"{Path}\")";
    }
    
    public static bool operator ==(ResourceReference left, ResourceReference right) => left.Equals(right);
    public static bool operator !=(ResourceReference left, ResourceReference right) => !(left == right);
}
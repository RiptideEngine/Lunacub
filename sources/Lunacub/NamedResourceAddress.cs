using Caxivitual.Lunacub.Serialization;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an named address of a specific resource in a specific library.
/// </summary>
[JsonConverter(typeof(ResourceNamedAddressConverter))]
public readonly struct NamedResourceAddress : IEquatable<NamedResourceAddress> {
    /// <summary>
    /// Represents a default or null value of the <see cref="NamedResourceAddress"/>, used to signify the absence of a
    /// valid resource.
    /// </summary>
    public static NamedResourceAddress Null => default;

    /// <summary>
    /// The id of the resource library.
    /// </summary>
    public readonly LibraryID LibraryId;

    /// <summary>
    /// The name of the resource.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Creates a new instance of <see cref="NamedResourceAddress"/> with the specified library id and resource name.
    /// </summary>
    /// <param name="libraryId">The id of the resource library to search for resource.</param>
    /// <param name="name">The name of resource.</param>
    public NamedResourceAddress(LibraryID libraryId, string name) {
        LibraryId = libraryId;
        Name = name;
    }

    /// <summary>
    /// Determine whether the address is deemed as null resource.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="LibraryId"/> is null (or equals to <see cref="LibraryID.Null"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsNull => LibraryId == LibraryID.Null;
    
    public bool Equals(NamedResourceAddress other) => LibraryId == other.LibraryId && Name == other.Name;

    public override string ToString() {
        return $"{{ LibraryId = {LibraryId}, Name = {Name} }}";
    }
    
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NamedResourceAddress other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(LibraryId, Name);
    
    public static bool operator ==(NamedResourceAddress left, NamedResourceAddress right) => left.Equals(right);
    public static bool operator !=(NamedResourceAddress left, NamedResourceAddress right) => !left.Equals(right);

    public static implicit operator SpanNamedResourceAddress(NamedResourceAddress address) => new(address.LibraryId, address.Name);
}
#pragma warning disable CS0660, CS0661

namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an named address of a specific resource in a specific library.
/// </summary>
public readonly ref struct SpanNamedResourceAddress : IEquatable<SpanNamedResourceAddress> {
    /// <summary>
    /// Represents a default or null value of the <see cref="SpanNamedResourceAddress"/>, used to signify the absence of a
    /// valid resource.
    /// </summary>
    public static SpanNamedResourceAddress Null => default;

    /// <summary>
    /// The id of the resource library.
    /// </summary>
    public readonly LibraryID LibraryId;

    /// <summary>
    /// The name of the resource.
    /// </summary>
    public readonly ReadOnlySpan<char> Name;

    /// <summary>
    /// Creates a new instance of <see cref="SpanNamedResourceAddress"/> with the specified library id and resource name.
    /// </summary>
    /// <param name="libraryId">The id of the resource library to search for resource.</param>
    /// <param name="name">The name of resource.</param>
    public SpanNamedResourceAddress(LibraryID libraryId, ReadOnlySpan<char> name) {
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
    
    public NamedResourceAddress ToNamedAddress() => new(LibraryId, Name.ToString());

    public void Deconstruct(out LibraryID libraryId, out ReadOnlySpan<char> name) {
        libraryId = LibraryId;
        name = Name;
    }
    
    public bool Equals(SpanNamedResourceAddress other) => LibraryId == other.LibraryId && Name == other.Name;

    public override string ToString() {
        return $"{{ LibraryId = {LibraryId}, Name = {Name} }}";
    }
    
    public static bool operator ==(SpanNamedResourceAddress left, SpanNamedResourceAddress right) => left.Equals(right);
    public static bool operator !=(SpanNamedResourceAddress left, SpanNamedResourceAddress right) => !left.Equals(right);
}
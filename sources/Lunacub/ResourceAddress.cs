namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an address of a specific resource in a specific library.
/// </summary>
public readonly struct ResourceAddress : IEquatable<ResourceAddress> {
    /// <summary>
    /// Represents a default or null value of the <see cref="ResourceAddress"/>, used to signify the absence of a
    /// valid resource.
    /// </summary>
    public static ResourceAddress Null => default;

    /// <summary>
    /// The id of the resource library to search for resource.
    /// </summary>
    public readonly LibraryID LibraryId;
    
    /// <summary>
    /// The id of the resource to search for.
    /// </summary>
    public readonly ResourceID ResourceId;

    /// <summary>
    /// Creates a new instance of <see cref="ResourceAddress"/> with the specified library id and resource id.
    /// </summary>
    /// <param name="libraryId">The id of the resource library to search for resource.</param>
    /// <param name="resourceId">The id of the resource to search for.</param>
    public ResourceAddress(LibraryID libraryId, ResourceID resourceId) {
        LibraryId = libraryId;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Determine whether the address is deemed as null resource.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="LibraryId"/> is null (or equals to <see cref="LibraryID.Null"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsNull => LibraryId == LibraryID.Null;
    
    public bool Equals(ResourceAddress other) => LibraryId == other.LibraryId && ResourceId == other.ResourceId;

    public override string ToString() {
        return $"{{ LibraryId = {LibraryId}, ResourceId = {ResourceId} }}";
    }
    
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceAddress other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(LibraryId, ResourceId);
    
    public static bool operator ==(ResourceAddress left, ResourceAddress right) => left.Equals(right);
    public static bool operator !=(ResourceAddress left, ResourceAddress right) => !left.Equals(right);
}
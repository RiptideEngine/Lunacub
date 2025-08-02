namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an address of a specific resource in a specific library.
/// </summary>
/// <param name="LibraryId">The id of the resource library.</param>
/// <param name="ResourceId">The id of the resource.</param>
public readonly record struct ResourceAddress(LibraryID LibraryId, ResourceID ResourceId) {
    /// <summary>
    /// Represents a default or null value of the <see cref="ResourceAddress"/>, used to signify the absence of a
    /// valid resource.
    /// </summary>
    public static ResourceAddress Null => default;

    /// <summary>
    /// Determine whether the address is deemed as null resource.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="LibraryId"/> is null (or equals to <see cref="LibraryID.Null"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsNull => LibraryId == LibraryID.Null;
}
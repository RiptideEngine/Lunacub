namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an address of a specific resource in a specific library.
/// </summary>
/// <param name="LibraryId">The id of the resource library.</param>
/// <param name="ResourceId">The id of the resource.</param>
public readonly record struct ResourceAddress(LibraryID LibraryId, ResourceID ResourceId);
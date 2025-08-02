using System.IO.Hashing;

namespace Caxivitual.Lunacub.Building;

public static class HashingHelpers {
    /// <summary>
    /// Compute the combined hash of the id of resource and the id of procedural resource.
    /// </summary>
    /// <param name="resourceId">The id of the building resource.</param>
    /// <param name="proceduralResourceID">The id of the currently generating procedural resource.</param>
    /// <returns>
    /// The combined <see cref="XxHash64"/> hash of <paramref name="resourceId"/> and <see cref="ProceduralResourceID"/>.
    /// </returns>
    [ExcludeFromCodeCoverage]
    public static ResourceID Combine(this ResourceID resourceId, ProceduralResourceID proceduralResourceID) {
        unsafe {
            Span<byte> buffer = stackalloc byte[sizeof(ResourceID) + sizeof(ProceduralResourceID)];

            fixed (byte* pointer = buffer) {
                Unsafe.Write(pointer, resourceId);
                Unsafe.Write(pointer + sizeof(ResourceID), proceduralResourceID);

                return XxHash64.HashToUInt64(buffer);
            }
        }
    }
}
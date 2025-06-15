using System.IO.Hashing;

namespace Caxivitual.Lunacub.Building;

public static class HashingHelpers {
    public static ResourceID Combine(this ResourceID resourceId, ProceduralResourceID proceduralResourceID) {
        unsafe {
            Span<byte> buffer = stackalloc byte[sizeof(ResourceID) + sizeof(ProceduralResourceID)];

            fixed (byte* pointer = buffer) {
                Unsafe.Write(pointer, resourceId);
                Unsafe.Write(pointer + sizeof(ResourceID), proceduralResourceID);

                return XxHash128.HashToUInt128(buffer);
            }
        }
    }
}
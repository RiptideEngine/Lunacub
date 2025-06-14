using System.Numerics;

namespace Caxivitual.Lunacub.Extensions;

public static class BinaryReaderExtensions {
    public static Vector2 ReadVector2(this BinaryReader reader) => reader.ReadReinterpret<Vector2>();
    public static Vector3 ReadVector3(this BinaryReader reader) => reader.ReadReinterpret<Vector3>();
    public static Vector4 ReadVector4(this BinaryReader reader) => reader.ReadReinterpret<Vector4>();
    public static Quaternion ReadQuaternion(this BinaryReader reader) => reader.ReadReinterpret<Quaternion>();
    public static Matrix3x2 ReadMatrix3x2(this BinaryReader reader) => reader.ReadReinterpret<Matrix3x2>();
    public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader) => reader.ReadReinterpret<Matrix4x4>();
    public static Plane ReadPlane(this BinaryReader reader) => reader.ReadReinterpret<Plane>();
    public static Guid ReadGuid(this BinaryReader reader) => reader.ReadReinterpret<Guid>();
    public static ResourceID ReadResourceID(this BinaryReader reader) => reader.ReadReinterpret<ResourceID>();

    public static T ReadReinterpret<T>(this BinaryReader reader) where T : unmanaged {
        unsafe {
            T output;
            reader.BaseStream.ReadExactly(new(&output, sizeof(T)));

            return output;
        }
    }

    public static void ReadReinterpret<T>(this BinaryReader reader, Span<T> output) where T : unmanaged {
        reader.BaseStream.ReadExactly(MemoryMarshal.AsBytes(output));
    }
}
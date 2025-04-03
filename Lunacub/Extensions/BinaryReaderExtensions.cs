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
    
    public static bool TryReadVector2(this BinaryReader reader, out Vector2 output) => reader.TryReadReinterpret(out output);
    public static bool TryReadVector3(this BinaryReader reader, out Vector3 output) => reader.TryReadReinterpret(out output);
    public static bool TryReadVector4(this BinaryReader reader, out Vector4 output) => reader.TryReadReinterpret(out output);
    public static bool TryReadQuaternion(this BinaryReader reader, out Quaternion output) => reader.TryReadReinterpret(out output);
    public static bool TryReadMatrix3x2(this BinaryReader reader, out Matrix3x2 output) => reader.TryReadReinterpret(out output);
    public static bool TryReadMatrix4x4(this BinaryReader reader, out Matrix4x4 output) => reader.TryReadReinterpret(out output);
    public static bool TryReadPlane(this BinaryReader reader, out Plane output) => reader.TryReadReinterpret(out output);
    public static bool TryReadGuid(this BinaryReader reader, out Guid output) => reader.TryReadReinterpret(out output);
    public static bool TryReadResourceID(this BinaryReader reader, out ResourceID output) => reader.TryReadReinterpret(out output);
    
    public static T ReadReinterpret<T>(this BinaryReader reader) where T : unmanaged {
        unsafe {
            T output;
            reader.BaseStream.ReadExactly(new(&output, sizeof(T)));

            return output;
        }
    }

    public static bool TryReadReinterpret<T>(this BinaryReader reader, out T output) where T : unmanaged {
        unsafe {
            fixed (T* ptr = &output) {
                if (reader.Read(new Span<byte>(ptr, sizeof(T))) != sizeof(T)) {
                    output = default;
                    return false;
                }

                return true;
            }
        }
    }
}
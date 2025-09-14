using Caxivitual.Lunacub.Compilation;
using System.Numerics;

namespace Caxivitual.Lunacub.Extensions;

public static class BinaryWriterExtensions {
    public static void Write(this BinaryWriter writer, Vector2 vector) => writer.WriteReinterpret(vector);
    public static void Write(this BinaryWriter writer, Vector3 vector) => writer.WriteReinterpret(vector);
    public static void Write(this BinaryWriter writer, Vector4 vector) => writer.WriteReinterpret(vector);
    public static void Write(this BinaryWriter writer, Quaternion quaternion) => writer.WriteReinterpret(quaternion);
    public static void Write(this BinaryWriter writer, Matrix3x2 matrix) => writer.WriteReinterpret(matrix);
    public static void Write(this BinaryWriter writer, Matrix4x4 matrix) => writer.WriteReinterpret(matrix);
    public static void Write(this BinaryWriter writer, Plane plane) => writer.WriteReinterpret(plane);
    public static void Write(this BinaryWriter writer, Guid guid) => writer.WriteReinterpret(guid);
    public static void Write(this BinaryWriter writer, LibraryID libraryId) => writer.WriteReinterpret(libraryId);
    public static void Write(this BinaryWriter writer, ResourceID resourceId) => writer.WriteReinterpret(resourceId);
    public static void Write(this BinaryWriter writer, ResourceAddress address) => writer.WriteReinterpret(address);
    public static void Write(this BinaryWriter writer, Tag tag) => writer.WriteReinterpret(tag);
    
    public static void WriteReinterpret<T>(this BinaryWriter writer, T value) where T : unmanaged {
        unsafe {
            writer.Write(new ReadOnlySpan<byte>(&value, sizeof(T)));
        }
    }

    public static void WriteReinterpret<T>(this BinaryWriter writer, ReadOnlySpan<T> span) where T : unmanaged {
        writer.Write(MemoryMarshal.AsBytes(span));
    }
}
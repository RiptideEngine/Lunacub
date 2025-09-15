using Caxivitual.Lunacub.Compilation;
using System.Numerics;

namespace Caxivitual.Lunacub.Extensions;

public static class BinaryWriterExtensions {
    public static void Write(this BinaryWriter writer, Vector2 vector) {
        writer.Write(vector.X);
        writer.Write(vector.Y);
    }

    public static void Write(this BinaryWriter writer, Vector3 vector) {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
    }

    public static void Write(this BinaryWriter writer, Vector4 vector) {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
        writer.Write(vector.W);
    }

    public static void Write(this BinaryWriter writer, Quaternion quaternion) {
        writer.Write(quaternion.X);
        writer.Write(quaternion.Y);
        writer.Write(quaternion.Z);
        writer.Write(quaternion.W);
    }

    public static void Write(this BinaryWriter writer, Matrix3x2 matrix) {
        writer.Write(matrix.M11); writer.Write(matrix.M12);
        writer.Write(matrix.M21); writer.Write(matrix.M22);
        writer.Write(matrix.M31); writer.Write(matrix.M32);
    }
    
    public static void Write(this BinaryWriter writer, Matrix4x4 matrix) {
        writer.Write(matrix.M11); writer.Write(matrix.M12); writer.Write(matrix.M13); writer.Write(matrix.M14);
        writer.Write(matrix.M21); writer.Write(matrix.M22); writer.Write(matrix.M23); writer.Write(matrix.M24);
        writer.Write(matrix.M31); writer.Write(matrix.M32); writer.Write(matrix.M33); writer.Write(matrix.M34);
        writer.Write(matrix.M41); writer.Write(matrix.M42); writer.Write(matrix.M43); writer.Write(matrix.M44);
    }

    public static void Write(this BinaryWriter writer, Plane plane) {
        writer.Write(plane.Normal);
        writer.Write(plane.D);
    }

    public static void Write(this BinaryWriter writer, Guid guid) {
        Span<byte> buffer = stackalloc byte[16];
        guid.TryWriteBytes(buffer);
        
        writer.Write(buffer);
    }

    public static void Write(this BinaryWriter writer, LibraryID libraryId) {
        writer.Write(libraryId.Value);
    }

    public static void Write(this BinaryWriter writer, ResourceID resourceId) {
        unsafe {
            Span<byte> buffer = stackalloc byte[sizeof(UInt128)];
            BinaryPrimitives.WriteUInt128LittleEndian(buffer, resourceId.Value);
            writer.Write(buffer);
        }
    }

    public static void Write(this BinaryWriter writer, ResourceAddress address) {
        writer.Write(address.LibraryId);
        writer.Write(address.ResourceId);
    }

    public static void Write(this BinaryWriter writer, Tag tag) {
        writer.Write(tag.AsSpan);
    }
}
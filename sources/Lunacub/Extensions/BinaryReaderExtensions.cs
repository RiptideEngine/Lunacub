using System.Numerics;

namespace Caxivitual.Lunacub.Extensions;

public static class BinaryReaderExtensions {
    public static Vector2 ReadVector2(this BinaryReader reader) {
        return new(reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector3 ReadVector3(this BinaryReader reader) {
        return new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Vector4 ReadVector4(this BinaryReader reader) {
        return new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Quaternion ReadQuaternion(this BinaryReader reader) {
        return new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static Matrix3x2 ReadMatrix3x2(this BinaryReader reader) {
        return new(
            reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle()
        );
    }

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader) {
        return new(
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
        );
    }
    public static Plane ReadPlane(this BinaryReader reader) {
        return new(reader.ReadVector4());
    }

    public static Guid ReadGuid(this BinaryReader reader) {
        Span<byte> buffer = stackalloc byte[16];
        reader.BaseStream.ReadExactly(buffer);

        return new(buffer);
    }

    public static LibraryID ReadLibraryID(this BinaryReader reader) {
        return reader.ReadUInt64();
    }

    public static ResourceID ReadResourceID(this BinaryReader reader) {
        Span<byte> buffer = stackalloc byte[16];
        reader.BaseStream.ReadExactly(buffer);

        return BinaryPrimitives.ReadUInt128LittleEndian(buffer);
    }

    public static ResourceAddress ReadResourceAddress(this BinaryReader reader) {
        return new(reader.ReadLibraryID(), reader.ReadResourceID());
    }
}
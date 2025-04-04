namespace Caxivitual.Lunacub.Tests;

public class BinaryReaderExtensionsTests : IDisposable {
    private readonly MemoryStream _ms;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;
    
    public BinaryReaderExtensionsTests() {
        _ms = new(64);
        _writer = new(_ms);
        _reader = new(_ms);
    }

    public void Dispose() {
        _writer.Dispose();
        _reader.Dispose();
        _ms.Dispose();
        
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ReadVector2_ShouldBeCorrect() {
        _writer.Write(new Vector2(2f, 2.5f));
        _ms.Position = 0;

        new Func<Vector2>(() => _reader.ReadVector2()).Should().NotThrow().Which.Should().Be(new Vector2(2f, 2.5f));
    }
    
    [Fact]
    public void ReadVector3_ShouldBeCorrect() {
        _writer.Write(new Vector3(2f, 2.5f, 4.45f));
        _ms.Position = 0;

        new Func<Vector3>(() => _reader.ReadVector3()).Should().NotThrow().Which.Should().Be(new Vector3(2f, 2.5f, 4.45f));
    }
    
    [Fact]
    public void ReadVector4_ShouldBeCorrect() {
        _writer.Write(new Vector4(2f, 2.5f, 4.45f, 9.55f));
        _ms.Position = 0;

        new Func<Vector4>(() => _reader.ReadVector4()).Should().NotThrow().Which.Should().Be(new Vector4(2f, 2.5f, 4.45f, 9.55f));
    }
    
    [Fact]
    public void ReadQuaternion_ShouldBeCorrect() {
        _writer.Write(Quaternion.CreateFromYawPitchRoll(0.65f, 1.11f, 3.14f));
        _ms.Position = 0;

        new Func<Quaternion>(() => _reader.ReadQuaternion()).Should().NotThrow().Which.Should().Be(Quaternion.CreateFromYawPitchRoll(0.65f, 1.11f, 3.14f));
    }
    
    [Fact]
    public void ReadMatrix3x2_ShouldBeCorrect() {
        _writer.Write(Matrix3x2.CreateRotation(125f));
        _ms.Position = 0;

        new Func<Matrix3x2>(() => _reader.ReadMatrix3x2()).Should().NotThrow().Which.Should().Be(Matrix3x2.CreateRotation(125f));
    }
    
    [Fact]
    public void ReadMatrix4x4_ShouldBeCorrect() {
        _writer.Write(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
        _ms.Position = 0;

        new Func<Matrix4x4>(() => _reader.ReadMatrix4x4()).Should().NotThrow().Which.Should().Be(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
    }
    
    [Fact]
    public void ReadPlane_ShouldBeCorrect() {
        _writer.Write(new Plane(Vector3.UnitX, 2));
        _ms.Position = 0;

        new Func<Plane>(() => _reader.ReadPlane()).Should().NotThrow().Which.Should().Be(new Plane(Vector3.UnitX, 2));
    }
    
    [Fact]
    public void ReadGuid_ShouldBeCorrect() {
        _writer.Write(Guid.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
        _ms.Position = 0;

        new Func<Guid>(() => _reader.ReadGuid()).Should().NotThrow().Which.Should().Be(Guid.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
    }
    
    [Fact]
    public void ReadResourceID_ShouldBeCorrect() {
        _writer.Write(ResourceID.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
        _ms.Position = 0;

        new Func<ResourceID>(() => _reader.ReadResourceID()).Should().NotThrow().Which.Should().Be(ResourceID.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
    }
    
    [Fact]
    public void ReadReinterpret_ShouldBeCorrect() {
        (Vector3 Position, Quaternion Rotation, Vector3 Scale) transform = (new(3, 2, 4), Quaternion.CreateFromYawPitchRoll(1.25f, 2.11f, 1.33f), new(1, 2, 4));
        _writer.WriteReinterpret(transform);
        _ms.Position = 0;

        new Func<(Vector3, Quaternion, Vector3)>(() => _reader.ReadReinterpret<(Vector3, Quaternion, Vector3)>()).Should().NotThrow().Which.Should().Be(transform);
    }
    
    [Fact]
    public void TryReadVector2_ShouldBeCorrect() {
        _writer.Write(new Vector2(2f, 2.5f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Vector2 output);
        new Func<bool>(() => _reader.TryReadVector2(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(new Vector2(2f, 2.5f));
    }
    
    [Fact]
    public void TryReadVector3_ShouldBeCorrect() {
        _writer.Write(new Vector3(2f, 2.5f, 4.45f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Vector3 output);
        new Func<bool>(() => _reader.TryReadVector3(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(new Vector3(2f, 2.5f, 4.45f));
    }
    
    [Fact]
    public void TryReadVector4_ShouldBeCorrect() {
        _writer.Write(new Vector4(2f, 2.5f, 4.45f, 9.55f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Vector4 output);
        new Func<bool>(() => _reader.TryReadVector4(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(new Vector4(2f, 2.5f, 4.45f, 9.55f));
    }
    
    [Fact]
    public void TryReadQuaternion_ShouldBeCorrect() {
        _writer.Write(Quaternion.CreateFromYawPitchRoll(0.65f, 1.11f, 3.14f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Quaternion output);
        new Func<bool>(() => _reader.TryReadQuaternion(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(Quaternion.CreateFromYawPitchRoll(0.65f, 1.11f, 3.14f));
    }
    
    [Fact]
    public void TryReadMatrix3x2_ShouldBeCorrect() {
        _writer.Write(Matrix3x2.CreateRotation(125f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Matrix3x2 output);
        new Func<bool>(() => _reader.TryReadMatrix3x2(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(Matrix3x2.CreateRotation(125f));
    }
    
    [Fact]
    public void TryReadMatrix4x4_ShouldBeCorrect() {
        _writer.Write(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
        _ms.Position = 0;

        Unsafe.SkipInit(out Matrix4x4 output);
        new Func<bool>(() => _reader.TryReadMatrix4x4(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
    }
    
    [Fact]
    public void TryReadPlane_ShouldBeCorrect() {
        _writer.Write(new Plane(Vector3.UnitX, 2));
        _ms.Position = 0;

        Unsafe.SkipInit(out Plane output);
        new Func<bool>(() => _reader.TryReadPlane(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(new Plane(Vector3.UnitX, 2));
    }
    
    [Fact]
    public void TryReadGuid_ShouldBeCorrect() {
        _writer.Write(Guid.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
        _ms.Position = 0;

        Unsafe.SkipInit(out Guid output);
        new Func<bool>(() => _reader.TryReadGuid(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(Guid.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
    }
    
    [Fact]
    public void TryReadResourceID_ShouldBeCorrect() {
        _writer.Write(ResourceID.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
        _ms.Position = 0;

        Unsafe.SkipInit(out ResourceID output);
        new Func<bool>(() => _reader.TryReadResourceID(out output)).Should().NotThrow().Which.Should().Be(true);
        output.Should().Be(ResourceID.Parse("17a7f0fe-c4c0-553a-a097-a48468eb2b90"));
    }
    
    [Fact]
    public void TryReadReinterpret_ShouldBeCorrect() {
        (Vector3 Position, Quaternion Rotation, Vector3 Scale) transform = (new(3, 2, 4), Quaternion.CreateFromYawPitchRoll(1.25f, 2.11f, 1.33f), new(1, 2, 4));
        _writer.WriteReinterpret(transform);
        _ms.Position = 0;

        Unsafe.SkipInit(out (Vector3, Quaternion, Vector3) output);
        new Func<bool>(() => _reader.TryReadReinterpret(out output)).Should().NotThrow().Which.Should().BeTrue();
        output.Should().Be(transform);
    }
    
    [Fact]
    public void TryReadVector2_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadVector2(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadVector3_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadVector3(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadVector4_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadVector4(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadQuaternion_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadQuaternion(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadMatrix3x2_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadMatrix3x2(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadMatrix4x4_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadMatrix4x4(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadPlane_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadPlane(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadGuid_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadGuid(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadResourceID_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadResourceID(out _)).Should().NotThrow().Which.Should().Be(false);
    }
    
    [Fact]
    public void TryReadReinterpret_Insufficient_ShouldReturnFalse() {
        _writer.Write((byte)0);
        _ms.Position = 0;

        new Func<bool>(() => _reader.TryReadReinterpret<Matrix4x4>(out _)).Should().NotThrow().Which.Should().Be(false);
    }
}
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
    public void ReadVector2_Sufficient_ReturnsOriginalValue() {
        _writer.Write(new Vector2(0f, 1f));
        _ms.Position = 0;

        new Func<Vector2>(() => _reader.ReadVector2()).Should().NotThrow().Which.Should().Be(new Vector2(0f, 1f));
    }

    [Fact]
    public void ReadVector2_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Vector2>(() => _reader.ReadVector2()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadVector3_Sufficient_ReturnsOriginalValue() {
        _writer.Write(new Vector3(0f, 1f, 2f));
        _ms.Position = 0;

        new Func<Vector3>(() => _reader.ReadVector3()).Should().NotThrow().Which.Should().Be(new Vector3(0f, 1f, 2f));
    }
    
    [Fact]
    public void ReadVector3_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Vector3>(() => _reader.ReadVector3()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadVector4_Sufficient_ReturnsOriginalValue() {
        _writer.Write(new Vector4(0f, 1f, 2f, 3f));
        _ms.Position = 0;

        new Func<Vector4>(() => _reader.ReadVector4()).Should().NotThrow().Which.Should().Be(new Vector4(0f, 1f, 2f, 3f));
    }
    
    [Fact]
    public void ReadVector4_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Vector4>(() => _reader.ReadVector4()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadQuaternion_Sufficient_ReturnsOriginalValue() {
        _writer.Write(Quaternion.CreateFromYawPitchRoll(0f, 1f, 2f));
        _ms.Position = 0;

        new Func<Quaternion>(() => _reader.ReadQuaternion()).Should().NotThrow().Which.Should().Be(Quaternion.CreateFromYawPitchRoll(0f, 1f, 2f));
    }
    
    [Fact]
    public void ReadQuaternion_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Quaternion>(() => _reader.ReadQuaternion()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadMatrix3x2_Sufficient_ReturnsOriginalValue() {
        _writer.Write(Matrix3x2.CreateRotation(125f));
        _ms.Position = 0;

        new Func<Matrix3x2>(() => _reader.ReadMatrix3x2()).Should().NotThrow().Which.Should().Be(Matrix3x2.CreateRotation(125f));
    }
    
    [Fact]
    public void ReadMatrix3x2_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Matrix3x2>(() => _reader.ReadMatrix3x2()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadMatrix4x4_Sufficient_ReturnsOriginalValue() {
        _writer.Write(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
        _ms.Position = 0;

        new Func<Matrix4x4>(() => _reader.ReadMatrix4x4()).Should().NotThrow().Which.Should().Be(Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f));
    }
    
    [Fact]
    public void ReadMatrix4x4_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Matrix4x4>(() => _reader.ReadMatrix4x4()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadPlane_Sufficient_ReturnsOriginalValue() {
        _writer.Write(new Plane(Vector3.UnitX, 2));
        _ms.Position = 0;

        new Func<Plane>(() => _reader.ReadPlane()).Should().NotThrow().Which.Should().Be(new Plane(Vector3.UnitX, 2));
    }
    
    [Fact]
    public void ReadPlane_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Plane>(() => _reader.ReadPlane()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadGuid_Sufficient_ReturnsOriginalValue() {
        Guid value = new("17a7f0fe-c4c0-553a-a097-a48468eb2b90");
        
        _writer.Write(value);
        _ms.Position = 0;

        new Func<Guid>(() => _reader.ReadGuid()).Should().NotThrow().Which.Should().Be(value);
    }
    
    [Fact]
    public void ReadGuid_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Guid>(() => _reader.ReadGuid()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadResourceID_Sufficient_ReturnsOriginalValue() {
        ResourceID value = new("17a7f0fec4c0553aa097a48468eb2b90");
        
        _writer.Write(value);
        _ms.Position = 0;

        new Func<ResourceID>(() => _reader.ReadResourceID()).Should().NotThrow().Which.Should().Be(value);
    }
    
    [Fact]
    public void ReadResourceID_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<ResourceID>(() => _reader.ReadResourceID()).Should().Throw<EndOfStreamException>();
    }
    
    [Fact]
    public void ReadReinterpret_Sufficient_ReturnsOriginalValue() {
        Transform transform = new(new(3, 2, 4), Quaternion.CreateFromYawPitchRoll(1.25f, 2.11f, 1.33f), new(1, 2, 4));
        _writer.WriteReinterpret(transform);
        _ms.Position = 0;

        new Func<Transform>(() => _reader.ReadReinterpret<Transform>()).Should().NotThrow().Which.Should().Be(transform);
    }
    
    [Fact]
    public void ReadReinterpret_Insufficient_ShouldThrowEndOfStreamException() {
        new Func<Matrix4x4>(() => _reader.ReadReinterpret<Matrix4x4>()).Should().Throw<EndOfStreamException>();
    }

    private readonly record struct Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale);
}
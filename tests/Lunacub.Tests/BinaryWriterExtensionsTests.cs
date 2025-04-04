namespace Caxivitual.Lunacub.Tests;

public class BinaryWriterExtensionsTests : IDisposable {
    private readonly byte[] _buffer;
    
    private readonly MemoryStream _ms;
    private readonly BinaryWriter _writer;
    
    public BinaryWriterExtensionsTests() {
        _buffer = new byte[64];
        _ms = new(_buffer, true);
        _writer = new(_ms);
    }

    public void Dispose() {
        _writer.Dispose();
        _ms.Dispose();
        
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void WriteVector2_ShouldBeCorrect() {
        _writer.Write(new Vector2(1.25f, 2.5f));

        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector2>());
        Unsafe.ReadUnaligned<Vector2>(ref _buffer[0]).Should().Be(new Vector2(1.25f, 2.5f));
    }
    
    [Fact]
    public void WriteVector3_ShouldBeCorrect() {
        _writer.Write(new Vector3(1.25f, 2.5f, 3.75f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector3>());
        Unsafe.ReadUnaligned<Vector3>(ref _buffer[0]).Should().Be(new Vector3(1.25f, 2.5f, 3.75f));
    }
    
    [Fact]
    public void WriteVector4_ShouldBeCorrect() {
        _writer.Write(new Vector4(1.25f, 2.5f, 3.75f, 5f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector4>());
        Unsafe.ReadUnaligned<Vector4>(ref _buffer[0]).Should().Be(new Vector4(1.25f, 2.5f, 3.75f, 5f));
    }
    
    [Fact]
    public void WriteQuaternion_ShouldBeCorrect() {
        _writer.Write(new Quaternion(1.25f, 2.5f, 3.75f, 5f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Quaternion>());
        Unsafe.ReadUnaligned<Quaternion>(ref _buffer[0]).Should().Be(new Quaternion(1.25f, 2.5f, 3.75f, 5f));
    }
    
    [Fact]
    public void WriteMatrix3x2_ShouldBeCorrect() {
        _writer.Write(new Matrix3x2(1.25f, 2.5f, 3.75f, 5f, 6.25f, 7.5f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Matrix3x2>());
        Unsafe.ReadUnaligned<Matrix3x2>(ref _buffer[0]).Should().Be(new Matrix3x2(1.25f, 2.5f, 3.75f, 5f, 6.25f, 7.5f));
    }
    
    [Fact]
    public void WriteMatrix4x4_ShouldBeCorrect() {
        _writer.Write(new Matrix4x4(0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f, 2.75f, 3f, 3.25f, 3.5f, 3.75f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Matrix4x4>());
        Unsafe.ReadUnaligned<Matrix4x4>(ref _buffer[0]).Should().Be(new Matrix4x4(0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f, 2.75f, 3f, 3.25f, 3.5f, 3.75f));
    }
    
    [Fact]
    public void WritePlane_ShouldBeCorrect() {
        _writer.Write(new Plane(Vector3.UnitX, 3));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Plane>());
        Unsafe.ReadUnaligned<Plane>(ref _buffer[0]).Should().Be(new Plane(Vector3.UnitX, 3));
    }
    
    [Fact]
    public void WriteGuid_ShouldBeCorrect() {
        _writer.Write(Guid.Parse("496c1989-55cf-587a-94b5-0ba89ca80d7d"));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Guid>());
        Unsafe.ReadUnaligned<Guid>(ref _buffer[0]).Should().Be(Guid.Parse("496c1989-55cf-587a-94b5-0ba89ca80d7d"));
    }
    
    [Fact]
    public void WriteResourceID_ShouldBeCorrect() {
        _writer.Write(ResourceID.Parse("496c1989-55cf-587a-94b5-0ba89ca80d7d"));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<ResourceID>());
        Unsafe.ReadUnaligned<ResourceID>(ref _buffer[0]).Should().Be(ResourceID.Parse("496c1989-55cf-587a-94b5-0ba89ca80d7d"));
    }

    [Fact]
    public void WriteReinterpret_ShouldBeCorrect() {
        (Vector3 Position, Quaternion Rotation, Vector3 Scale) transform = (new(3, 2, 4), Quaternion.CreateFromYawPitchRoll(1.25f, 2.11f, 1.33f), new(1, 2, 4));
        _writer.WriteReinterpret(transform);
        
        _writer.BaseStream.Position.Should().Be(40);
        Unsafe.ReadUnaligned<(Vector3, Quaternion, Vector3)>(ref _buffer[0]).Should().Be(transform);
    }
}
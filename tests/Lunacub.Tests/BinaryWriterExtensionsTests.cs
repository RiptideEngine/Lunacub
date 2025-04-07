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
    public void Write_Vector2_RecordOriginalValue() {
        _writer.Write(new Vector2(0f, 1f));

        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector2>());
        Unsafe.ReadUnaligned<Vector2>(ref _buffer[0]).Should().Be(new Vector2(0f, 1f));
    }
    
    [Fact]
    public void Write_Vector3_RecordOriginalValue() {
        _writer.Write(new Vector3(0f, 1f, 2f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector3>());
        Unsafe.ReadUnaligned<Vector3>(ref _buffer[0]).Should().Be(new Vector3(0f, 1f, 2f));
    }
    
    [Fact]
    public void Write_Vector4_RecordOriginalValue() {
        _writer.Write(new Vector4(0f, 1f, 2f, 3f));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Vector4>());
        Unsafe.ReadUnaligned<Vector4>(ref _buffer[0]).Should().Be(new Vector4(0f, 1f, 2f, 3f));
    }
    
    [Fact]
    public void Write_Quaternion_RecordOriginalValue() {
        Quaternion value = Quaternion.CreateFromYawPitchRoll(0f, 1f, 2f);
        _writer.Write(value);
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Quaternion>());
        Unsafe.ReadUnaligned<Quaternion>(ref _buffer[0]).Should().Be(value);
    }
    
    [Fact]
    public void Write_Matrix3x2_RecordOriginalValue() {
        Matrix3x2 value = Matrix3x2.CreateRotation(95f);
        _writer.Write(value);
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Matrix3x2>());
        Unsafe.ReadUnaligned<Matrix3x2>(ref _buffer[0]).Should().Be(value);
    }
    
    [Fact]
    public void Write_Matrix4x4_RecordOriginalValue() {
        Matrix4x4 value = Matrix4x4.CreateFromYawPitchRoll(0f, 1f, 2f);
        _writer.Write(value);
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Matrix4x4>());
        Unsafe.ReadUnaligned<Matrix4x4>(ref _buffer[0]).Should().Be(value);
    }
    
    [Fact]
    public void Write_Plane_RecordOriginalValue() {
        _writer.Write(new Plane(Vector3.UnitX, 3));
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Plane>());
        Unsafe.ReadUnaligned<Plane>(ref _buffer[0]).Should().Be(new Plane(Vector3.UnitX, 3));
    }
    
    [Fact]
    public void Write_Guid_RecordOriginalValue() {
        Guid value = new("496c1989-55cf-587a-94b5-0ba89ca80d7d");
        _writer.Write(value);
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<Guid>());
        Unsafe.ReadUnaligned<Guid>(ref _buffer[0]).Should().Be(value);
    }
    
    [Fact]
    public void Write_ResourceID_RecordOriginalValue() {
        ResourceID value = new("496c1989-55cf-587a-94b5-0ba89ca80d7d");
        _writer.Write(value);
        
        _writer.BaseStream.Position.Should().Be(Unsafe.SizeOf<ResourceID>());
        Unsafe.ReadUnaligned<ResourceID>(ref _buffer[0]).Should().Be(value);
    }

    [Fact]
    public void Write_Reinterpret_RecordOriginalValue() {
        Transform transform = new(new(3, 2, 4), Quaternion.CreateFromYawPitchRoll(1.25f, 2.11f, 1.33f), new(1, 2, 4));
        _writer.WriteReinterpret(transform);
        
        _writer.BaseStream.Position.Should().Be(40);
        Unsafe.ReadUnaligned<Transform>(ref _buffer[0]).Should().Be(transform);
    }
    
    private readonly record struct Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale);
}
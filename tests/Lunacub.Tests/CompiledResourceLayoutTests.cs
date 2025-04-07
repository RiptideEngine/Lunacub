using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Tests;

public class CompiledResourceLayoutTests {
    [Fact]
    public void TryGetChunkInformation_ShouldBeCorrect() {
        CompiledResourceLayout layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
            new(BinaryPrimitives.ReadUInt32LittleEndian("EFGH"u8), 8, 4),
            new(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12),
            new(BinaryPrimitives.ReadUInt32LittleEndian("MNOP"u8), 16, 24),
        ]);
        
        layout.TryGetChunkInformation("IJKL"u8, out var info).Should().BeTrue();
        info.Length.Should().Be(12);
        info.Length.Should().Be(12);
        
        layout.TryGetChunkInformation("QRST"u8, out _).Should().BeFalse();
    }
    
    [Fact]
    public void TryGetChunkInformation_ShouldReturnFalseOnInvalidTag() {
        CompiledResourceLayout layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
        ]);

        layout.TryGetChunkInformation([], out _).Should().BeFalse();
        layout.TryGetChunkInformation("?????"u8, out _).Should().BeFalse();
    }
}
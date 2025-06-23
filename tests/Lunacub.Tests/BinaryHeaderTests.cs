using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Tests;

public class BinaryHeaderTests {
    [Fact]
    public void TryGetChunkInformation_RegisteredTag_ReturnsStoredValue() {
        BinaryHeader layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
            new(BinaryPrimitives.ReadUInt32LittleEndian("EFGH"u8), 8, 4),
            new(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12),
            new(BinaryPrimitives.ReadUInt32LittleEndian("MNOP"u8), 16, 24),
        ]);
        
        layout.TryGetChunkInformation("IJKL"u8, out var info).Should().BeTrue();
        info.Should().Be(new ChunkInformation(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12));
    }

    [Fact]
    public void TryGetChunkInformation_UnregisteredTag_ReturnsFalseAndOutputDefault() {
        BinaryHeader layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
            new(BinaryPrimitives.ReadUInt32LittleEndian("EFGH"u8), 8, 4),
            new(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12),
            new(BinaryPrimitives.ReadUInt32LittleEndian("MNOP"u8), 16, 24),
        ]);
        
        layout.TryGetChunkInformation("QRST"u8, out var output).Should().BeFalse();
        output.Should().Be(new ChunkInformation(0, 0, 0));
    }

    public static IEnumerable<object[]> GetInvalidTagData() {
        yield return [Array.Empty<byte>()];
        yield return ["?????"u8.ToArray()];
    }

    [Theory, MemberData(nameof(GetInvalidTagData))]
    public void TryGetChunkInformation_InvalidTag_ReturnsFalseAndOutputDefault(ReadOnlyMemory<byte> tag) {
        BinaryHeader layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
        ]);

        layout.TryGetChunkInformation(tag.Span, out var output).Should().BeFalse();
        output.Should().Be(new ChunkInformation(0, 0, 0));
    }
}
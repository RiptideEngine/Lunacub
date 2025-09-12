// ReSharper disable AccessToDisposedClosure

using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Helpers;
using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Tests;

public class HeaderTests {
    [Fact]
    public void TryGetChunkInformation_RegisteredTag_ReturnsStoredValue() {
        Header layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
            new(BinaryPrimitives.ReadUInt32LittleEndian("EFGH"u8), 8, 4),
            new(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12),
            new(BinaryPrimitives.ReadUInt32LittleEndian("MNOP"u8), 16, 24),
        ]);
        
        layout.TryGetChunkInformation("IJKL"u8, out var info).Should().BeTrue();
        info.Should().Be(new ChunkPositionalInformation(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12));
    }

    [Fact]
    public void TryGetChunkInformation_UnregisteredTag_ReturnsFalseAndOutputDefault() {
        Header layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
            new(BinaryPrimitives.ReadUInt32LittleEndian("EFGH"u8), 8, 4),
            new(BinaryPrimitives.ReadUInt32LittleEndian("IJKL"u8), 12, 12),
            new(BinaryPrimitives.ReadUInt32LittleEndian("MNOP"u8), 16, 24),
        ]);
        
        layout.TryGetChunkInformation("QRST"u8, out var output).Should().BeFalse();
        output.Should().Be(new ChunkPositionalInformation(0, 0, 0));
    }

    public static TheoryData<byte[]> InvalidTags => new() {
        Array.Empty<byte>(),
        "????"u8.ToArray(),
    };

    [Theory, MemberData(nameof(InvalidTags))]
    public void TryGetChunkInformation_InvalidTag_ReturnsFalseAndOutputDefault(byte[] tag) {
        Header layout = new(1, 0, [
            new(BinaryPrimitives.ReadUInt32LittleEndian("ABCD"u8), 4, 0),
        ]);

        layout.TryGetChunkInformation(tag, out var output).Should().BeFalse();
        output.Should().Be(new ChunkPositionalInformation(0, 0, 0));
    }
    
    [Fact]
    public void Extract_FromStream_ReturnsCorrectLayout() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write("FAKE"u8);
            writer.Write(20);
            writer.Write("FAKE"u8);
            writer.Write(16);
            writer.Write(Enumerable.Repeat((byte)0, 16).ToArray());
        }

        ms.Seek(0, SeekOrigin.Begin);

        Header layout = new Func<Header>(() => Header.Extract(ms))
            .Should().NotThrow().Which;
        
        layout.MajorVersion.Should().Be(1);
        layout.MinorVersion.Should().Be(0);
        layout.Chunks.Should().ContainSingle();
        layout.Chunks[0].Should().Be(new ChunkPositionalInformation(BinaryPrimitives.ReadUInt32LittleEndian("FAKE"u8), 16, 28));
    }

    [Fact]
    public void Extract_FromMemory_ReturnsCorrectLayout() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write("FAKE"u8);
            writer.Write(20);
            writer.Write("FAKE"u8);
            writer.Write(16);
            writer.Write(Enumerable.Repeat((byte)0, 16).ToArray());
        }

        ms.Seek(0, SeekOrigin.Begin);

        Header layout = new Func<Header>(() => Header.Extract(ms))
            .Should().NotThrow().Which;
        
        layout.MajorVersion.Should().Be(1);
        layout.MinorVersion.Should().Be(0);
        layout.Chunks.Should().ContainSingle();
        layout.Chunks[0].Should().Be(new ChunkPositionalInformation(BinaryPrimitives.ReadUInt32LittleEndian("FAKE"u8), 16, 28));
    }
    
    [Fact]
    public void Extract_UnreadableStream_ThrowsArgumentException() {
        MemoryStream ms = new MemoryStream([]);
        ms.Dispose();

        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<ArgumentException>().WithMessage("*Stream*readable*");
    }

    [Fact]
    public void Extract_InvalidMagic_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream("\0\0\0\0\0\0\0\0\0\0\0\0"u8.ToArray());
        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<CorruptedBinaryException>().WithMessage("*magic*");
    }
    
    [Fact]
    public void Extract_InsufficientHeader_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
        }

        ms.Seek(0, SeekOrigin.Begin);

        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<CorruptedBinaryException>().WithMessage("*not*sufficient*read*header*");
    }

    [Fact]
    public void Extract_ChunkPositionSurpassedStreamLength_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write(CompilingConstants.ResourceDataChunkTag);
            writer.Write(int.MaxValue);
        }

        ms.Seek(0, SeekOrigin.Begin);
        
        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<CorruptedBinaryException>().WithMessage("*surpassed*length*");
    }
    
    [Fact]
    public void Extract_UnexpectedChunkTag_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write("FAKE"u8);
            writer.Write(20);
            writer.Write("DIFF"u8);
            writer.Write(0);
        }

        ms.Seek(0, SeekOrigin.Begin);

        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<CorruptedBinaryException>().WithMessage("*Expected*chunk tag*position*");
    }
    
    [Fact]
    public void Extract_ChunkContentSurpassedStreamLength_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write("FAKE"u8);
            writer.Write(20);
            writer.Write("FAKE"u8);
            writer.Write(int.MaxValue);
        }

        ms.Seek(0, SeekOrigin.Begin);

        new Func<Header>(() => Header.Extract(ms))
            .Should().Throw<CorruptedBinaryException>().WithMessage("*content*length*surpassed*");
    }
}
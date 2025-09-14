// ReSharper disable AccessToDisposedClosure

using Caxivitual.Lunacub.Helpers;
using Caxivitual.Lunacub.Tests.Common;
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Tests;

public unsafe class HeaderHelpersTests {
    private readonly ImmutableArray<ChunkOffset> _offsets = [
        new("ABCD"u8, 20),
        new("EFGH"u8, 28),
        new("IJKL"u8, 36),
        new("MNOP"u8, 44),
    ];
    
    private readonly ImmutableArray<ChunkLength> _lengths = [
        new("ABCD"u8, 20),
        new("EFGH"u8, 28),
        new("IJKL"u8, 36),
        new("MNOP"u8, 44),
    ];
    
    [Fact]
    public void TryGetOffsetFromMemory_Exists_ReturnsCorrectResults() {
        ReadOnlyMemory<ChunkOffset> memory = _offsets.AsMemory();

        bool result = memory.TryGet("ABCD"u8, out ChunkOffset offset);
        
        result.Should().BeTrue();
        offset.Should().Be(new ChunkOffset("ABCD"u8, 20));
    }
    
    [Fact]
    public void TryGetOffsetFromMemory_NotExists_ReturnsFalseAndDefault() {
        ReadOnlyMemory<ChunkOffset> memory = _offsets.AsMemory();

        bool result = memory.TryGet("CDEF"u8, out ChunkOffset offset);
        
        result.Should().BeFalse();
        offset.Should().Be(default(ChunkOffset));
    }
    
    [Fact]
    public void TryGetOffsetFromSpan_Exists_ReturnsCorrectResults() {
        ReadOnlySpan<ChunkOffset> span = _offsets.AsSpan();

        bool result = span.TryGet("ABCD"u8, out ChunkOffset offset);
        
        result.Should().BeTrue();
        offset.Should().Be(new ChunkOffset("ABCD"u8, 20));
    }
    
    [Fact]
    public void TryGetOffsetFromSpan_NotExists_ReturnsFalseAndDefault() {
        ReadOnlySpan<ChunkOffset> span = _offsets.AsSpan();

        bool result = span.TryGet("CDEF"u8, out ChunkOffset offset);
        
        result.Should().BeFalse();
        offset.Should().Be(default(ChunkOffset));
    }
    
    [Fact]
    public void TryGetLengthFromMemory_Exists_ReturnsCorrectResults() {
        ReadOnlyMemory<ChunkLength> memory = _lengths.AsMemory();

        bool result = memory.TryGet("ABCD"u8, out ChunkLength offset);
        
        result.Should().BeTrue();
        offset.Should().Be(new ChunkLength("ABCD"u8, 20));
    }
    
    [Fact]
    public void TryGetLengthFromMemory_NotExists_ReturnsFalseAndDefault() {
        ReadOnlyMemory<ChunkLength> memory = _lengths.AsMemory();

        bool result = memory.TryGet("CDEF"u8, out ChunkLength offset);
        
        result.Should().BeFalse();
        offset.Should().Be(default(ChunkLength));
    }
    
    [Fact]
    public void TryGetLengthFromSpan_Exists_ReturnsCorrectResults() {
        ReadOnlySpan<ChunkLength> span = _lengths.AsSpan();

        bool result = span.TryGet("ABCD"u8, out ChunkLength offset);
        
        result.Should().BeTrue();
        offset.Should().Be(new ChunkLength("ABCD"u8, 20));
    }
    
    [Fact]
    public void TryGetLengthFromSpan_NotExists_ReturnsFalseAndDefault() {
        ReadOnlySpan<ChunkLength> span = _lengths.AsSpan();

        bool result = span.TryGet("CDEF"u8, out ChunkLength offset);
        
        result.Should().BeFalse();
        offset.Should().Be(default(ChunkLength));
    }
    
    [Fact]
    public void Extract_FromStream_ReturnsCorrectLayout() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(2); // 2 chunks
            writer.Write("ABCD"u8);
            writer.Write(4);
            writer.Write("EFGH"u8);
            writer.Write(8);
        }

        ms.Seek(0, SeekOrigin.Begin);

        Header layout = new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().NotThrow().Which;
        
        layout.MajorVersion.Should().Be(1);
        layout.MinorVersion.Should().Be(0);
        layout.ChunkOffsets.Should().Equal(ImmutableArray.Create(new ChunkOffset("ABCD"u8, 4), new ChunkOffset("EFGH"u8, 8)));
    }
    
    [Fact]
    public void Extract_UnreadableStream_ThrowsArgumentException() {
        using ConfigurableStream stream = new ConfigurableStream(false, false, true);

        new Func<Header>(() => HeaderHelpers.Extract(stream))
            .Should().Throw<ArgumentException>().WithMessage("*Stream*readable*");
    }

    [Fact]
    public void Extract_InvalidMagic_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream("\0\0\0\0\0\0\0\0\0\0\0\0"u8.ToArray());
        new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().Throw<InvalidDataException>().WithMessage("*magic*");
    }
    
    [Fact]
    public void Extract_InsufficientStreamForMagic_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();
    
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write("AB"u8);
        }
    
        ms.Seek(0, SeekOrigin.Begin);
        
        new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().Throw<EndOfStreamException>().WithMessage("*magic*");
    }
    
    [Fact]
    public void Extract_InsufficientStreamForVersion_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();
    
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
        }
    
        ms.Seek(0, SeekOrigin.Begin);
    
        new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().Throw<EndOfStreamException>().WithMessage("*version*");
    }
    
    [Fact]
    public void Extract_InsufficientStreamForChunkAmount_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();
    
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
        }
    
        ms.Seek(0, SeekOrigin.Begin);
    
        new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().Throw<EndOfStreamException>().WithMessage("*chunk*");
    }
    
    [Fact]
    public void Extract_InsufficientStreamForChunkOffsets_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();
    
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(16);
            writer.Write("DATA"u8);
            writer.Write(0);
        }
    
        ms.Seek(0, SeekOrigin.Begin);
    
        new Func<Header>(() => HeaderHelpers.Extract(ms))
            .Should().Throw<EndOfStreamException>().WithMessage("*chunk offsets*");
    }

    [Fact]
    public void ExtractChunkPositionalInfo_CorrectBinary_ReturnsCorrectly() {
        using MemoryStream ms = new MemoryStream();
        
        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(3);
            writer.Write("CHK1"u8);
            writer.Write(36);
            writer.Write("CHK2"u8);
            writer.Write(52);
            writer.Write("CHK3"u8);
            writer.Write(72);
            writer.Write("CHK1"u8);
            writer.Write(sizeof(Vector2));
            writer.Write(Vector2.Zero);
            writer.Write("CHK2"u8);
            writer.Write(sizeof(Vector3));
            writer.Write(Vector3.Zero);
            writer.Write("CHK3"u8);
            writer.Write(sizeof(Vector4));
            writer.Write(Vector4.Zero);
        }
        
        ms.Seek(0, SeekOrigin.Begin);

        Header header = new Func<Header>(() => HeaderHelpers.Extract(ms)).Should().NotThrow().Which;

        ChunkPositionalInformation[] positionalInfos = new ChunkPositionalInformation[header.ChunkOffsets.Length];
        new Action(() => HeaderHelpers.ExtractChunkPositionalInfo(header.ChunkOffsets.AsSpan(), ms, positionalInfos)).Should().NotThrow();

        positionalInfos.Should().Equal([
            new("CHK1"u8, 36, (uint)sizeof(Vector2), 44),
            new("CHK2"u8, 52, (uint)sizeof(Vector3), 60),
            new("CHK3"u8, 72, (uint)sizeof(Vector4), 80),
        ]);
    }
    
    // [Fact]
    // public void Extract_ChunkContentSurpassedStreamLength_ThrowsCorruptedFormatException() {
    //     using MemoryStream ms = new MemoryStream();
    //
    //     using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
    //         writer.Write(CompilingConstants.MagicIdentifier);
    //         writer.Write((ushort)1);
    //         writer.Write((ushort)0);
    //         writer.Write(1); // 1 chunks
    //         writer.Write("C001"u8);
    //         writer.Write(20);
    //         writer.Write("C002"u8);
    //         writer.Write(int.MaxValue);
    //     }
    //
    //     ms.Seek(0, SeekOrigin.Begin);
    //
    //     new Func<Header>(() => HeaderHelpers.Extract(ms))
    //         .Should().Throw<InvalidDataException>().WithMessage("*content*length*surpassed*");
    // }
}
﻿using Caxivitual.Lunacub.Exceptions;
using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Tests;

public class LayoutValidationTests {
    [Fact]
    public void Validate_FromStream_ReturnsCorrectLayout() {
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

        CompiledResourceLayout layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().NotThrow().Which;
        
        layout.MajorVersion.Should().Be(1);
        layout.MinorVersion.Should().Be(0);
        layout.Chunks.Should().ContainSingle();
        layout.Chunks[0].Should().Be(new ChunkInformation(BinaryPrimitives.ReadUInt32LittleEndian("FAKE"u8), 16, 28));
    }

    [Fact]
    public void Validate_FromMemory_ReturnsCorrectLayout() {
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

        CompiledResourceLayout layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms.ToArray()))
            .Should().NotThrow().Which;
        
        layout.MajorVersion.Should().Be(1);
        layout.MinorVersion.Should().Be(0);
        layout.Chunks.Should().ContainSingle();
        layout.Chunks[0].Should().Be(new ChunkInformation(BinaryPrimitives.ReadUInt32LittleEndian("FAKE"u8), 16, 28));
    }
    
    [Fact]
    public void Validate_UnreadableStream_ThrowsArgumentException() {
        MemoryStream ms = new MemoryStream([]);
        ms.Dispose();

        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<ArgumentException>().WithMessage("*Stream*readable*");
    }

    [Fact]
    public void Validate_InvalidMagic_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream("????"u8.ToArray());
        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*magic*");
    }
    
    [Fact]
    public void Validate_HeaderEndOfStream_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
        }
    
        ms.Seek(0, SeekOrigin.Begin);
        
        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*header*").WithInnerException<EndOfStreamException>();
    }
    
    [Fact]
    public void Validate_ChunkHeaderEndOfStream_ThrowsCorruptedFormatException() {
        using MemoryStream ms = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            writer.Write(CompilingConstants.MagicIdentifier);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write(1); // 1 chunks
            writer.Write("FAKE"u8);
            writer.Write(20);
            writer.Write("FAKE"u8);
            writer.Write((ushort)0);
        }

        ms.Seek(0, SeekOrigin.Begin);
        
        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*chunk header*").WithInnerException<EndOfStreamException>();
    }

    [Fact]
    public void Validate_ChunkPositionSurpassedStreamLength_ThrowsCorruptedFormatException() {
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
        
        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*surpassed*length*");
    }
    
    [Fact]
    public void Validate_UnexpectedChunkTag_ThrowsCorruptedFormatException() {
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

        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*Expected*chunk tag*position*");
    }
    
    [Fact]
    public void Validate_ChunkContentSurpassedStreamLength_ThrowsCorruptedFormatException() {
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

        new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(ms))
            .Should().Throw<CorruptedFormatException>().WithMessage("*content*length*surpassed*");
    }
}
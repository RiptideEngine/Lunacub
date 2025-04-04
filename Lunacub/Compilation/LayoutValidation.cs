using Caxivitual.Lunacub.Exceptions;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public static class LayoutValidation {
    public static CompiledResourceLayout Validate(Stream stream) {
        if (!stream.CanRead) throw new ArgumentException("Stream is not readable.");
        if (!stream.CanSeek) throw new ArgumentException("Stream is not seekable.");
        
        using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true);

        uint magic = reader.ReadUInt32();

        if (magic != BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.MagicIdentifier)) {
            throw new CorruptedFormatException($"Unexpected magic identifier 0x{magic:x8}.");
        }
        
        ushort majorVersion = reader.ReadUInt16();
        ushort minorVersion = reader.ReadUInt16();

        int chunkAmount = reader.ReadInt32();
        
        Span<KeyValuePair<uint, int>> chunkPositions = chunkAmount > 16 ? new KeyValuePair<uint, int>[chunkAmount] : stackalloc KeyValuePair<uint, int>[chunkAmount];

        stream.ReadExactly(MemoryMarshal.AsBytes(chunkPositions));
        
        var chunkInfoBuilder = ImmutableArray.CreateBuilder<ChunkInformation>(chunkAmount);

        foreach ((uint chunkTag, int position) in chunkPositions) {
            ReadOnlySpan<byte> chunkTagBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in chunkTag, 1));
            
            if (position >= stream.Length) throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has position surpassed Stream's length.");

            stream.Seek(position, SeekOrigin.Begin);

            uint validatingChunk;
            
            try {
                validatingChunk = reader.ReadUInt32();
            } catch (EndOfStreamException e) {
                throw new CorruptedFormatException($"Failed to validate chunk {Encoding.ASCII.GetString(chunkTagBytes)} (Unreadable tag).", e);
            }

            if (validatingChunk != chunkTag) {
                throw new CorruptedFormatException($"Expected chunk tag {Encoding.ASCII.GetString(chunkTagBytes)} at position {position}.");
            }
            
            uint chunkLength;
            
            try {
                chunkLength = reader.ReadUInt32();
            } catch (EndOfStreamException e) {
                throw new CorruptedFormatException($"Failed to validate chunk {Encoding.ASCII.GetString(chunkTagBytes)} (Unreadable length).", e);
            }

            if (stream.Position + chunkLength > stream.Length) {
                throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has length surpassed Stream's length.");
            }
            
            chunkInfoBuilder.Add(new(chunkTag, chunkLength, stream.Position));
        }
        
        return new(majorVersion, minorVersion, chunkInfoBuilder.MoveToImmutable());
    }

    public static CompiledResourceLayout Validate(ReadOnlySpan<byte> memory) {
        unsafe {
            fixed (byte* ptr = memory) {
                using UnmanagedMemoryStream stream = new(ptr, memory.Length, 0, FileAccess.Read);
                return Validate(stream);
            }
        }
    }
}
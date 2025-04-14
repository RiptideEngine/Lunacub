using Caxivitual.Lunacub.Exceptions;
using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public static class LayoutValidation {
    public static CompiledResourceLayout Validate(Stream stream) {
        if (!stream.CanRead || !stream.CanSeek) throw new ArgumentException("Stream is not readable or not seekable.");
        
        using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true);

        KeyValuePair<uint, int>[] chunkPositions = [];

        ushort majorVersion, minorVersion;
        int chunkAmount;
        try {
            uint magic = reader.ReadUInt32();

            if (magic != BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.MagicIdentifier)) {
                throw new CorruptedFormatException($"Unexpected magic identifier 0x{magic:x8}.");
            }

            majorVersion = reader.ReadUInt16();
            minorVersion = reader.ReadUInt16();
            chunkAmount = reader.ReadInt32();
            
            chunkPositions = ArrayPool<KeyValuePair<uint, int>>.Shared.Rent(chunkAmount);

            stream.ReadExactly(MemoryMarshal.AsBytes(chunkPositions.AsSpan(0, chunkAmount)));
        } catch (EndOfStreamException e) {
            ArrayPool<KeyValuePair<uint, int>>.Shared.Return(chunkPositions);
            throw new CorruptedFormatException("Unable to read compiled resource header.", e);
        }

        try {
            var chunkInfoBuilder = ImmutableArray.CreateBuilder<ChunkInformation>(chunkAmount);

            ValidateChunkInformations(reader, chunkPositions.AsSpan(0, chunkAmount), chunkInfoBuilder);

            return new(majorVersion, minorVersion, chunkInfoBuilder.MoveToImmutable());
        } finally {
            ArrayPool<KeyValuePair<uint, int>>.Shared.Return(chunkPositions);
        }
    }

    public static CompiledResourceLayout Validate(ReadOnlySpan<byte> memory) {
        unsafe {
            fixed (byte* ptr = memory) {
                using UnmanagedMemoryStream stream = new(ptr, memory.Length, memory.Length, FileAccess.Read);
                return Validate(stream);
            }
        }
    }

    private static unsafe void ValidateChunkInformations(BinaryReader reader, ReadOnlySpan<KeyValuePair<uint, int>> chunkPositions, ImmutableArray<ChunkInformation>.Builder chunkInfoBuilder) {
        Stream stream = reader.BaseStream;
        
        foreach ((uint chunkTag, int position) in chunkPositions) {
            ReadOnlySpan<byte> chunkTagBytes = new ReadOnlySpan<byte>(&chunkTag, sizeof(uint));
            
            if (position >= stream.Length) throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has position surpassed Stream's length.");
            
            stream.Seek(position, SeekOrigin.Begin);

            try {
                uint validatingChunk = reader.ReadUInt32();
                
                if (validatingChunk != chunkTag) {
                    throw new CorruptedFormatException($"Expected chunk tag {Encoding.ASCII.GetString(chunkTagBytes)} at position {position}.");
                }
                
                uint contentLength = reader.ReadUInt32();
                
                if (stream.Position + contentLength > stream.Length) {
                    throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has content length surpassed Stream's length.");
                }
                
                chunkInfoBuilder.Add(new(chunkTag, contentLength, stream.Position));
            } catch (EndOfStreamException e) {
                throw new CorruptedFormatException($"Failed to read chunk header of chunk {Encoding.ASCII.GetString(chunkTagBytes)}.", e);
            }
        }
    }
}
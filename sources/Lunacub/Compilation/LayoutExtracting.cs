using Caxivitual.Lunacub.Exceptions;
using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public static class LayoutExtracting {
    public static BinaryHeader ExtractHeader(Stream stream) {
        if (!stream.CanRead || !stream.CanSeek) throw new ArgumentException("Stream is not readable or not seekable.");

        Span<byte> header = stackalloc byte[12];

        if (stream.Read(header) < 12) {
            throw new CorruptedBinaryException("Stream does not contain sufficient data to read binary header.");
        }

        if (!header.StartsWith(CompilingConstants.MagicIdentifier)) {
            throw new CorruptedBinaryException("Unexpected magic identifier.");
        }

        ushort major = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(4, 2));
        ushort minor = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(6, 2));
        int chunkCount = BinaryPrimitives.ReadInt32LittleEndian(header.Slice(8, 4));

        KeyValuePair<uint, uint>[]? rentedArray = null;
        Span<KeyValuePair<uint, uint>> span = chunkCount <= 16 ?
            stackalloc KeyValuePair<uint, uint>[chunkCount] :
            (rentedArray = ArrayPool<KeyValuePair<uint, uint>>.Shared.Rent(chunkCount)).AsSpan()[..chunkCount];

        try {
            if (stream.Read(MemoryMarshal.AsBytes(span)) < chunkCount * sizeof(uint) * 2) {
                throw new CorruptedBinaryException("Stream does not contain sufficient data to read chunk offsets.");
            }

            var builder = ImmutableArray.CreateBuilder<ChunkInformation>(chunkCount);
            
            ValidateChunkInformations(stream, span, builder);
            
            return new(major, minor, builder.MoveToImmutable());
        } finally {
            if (rentedArray != null) {
                ArrayPool<KeyValuePair<uint, uint>>.Shared.Return(rentedArray);
            }
        }
    }

    private static unsafe void ValidateChunkInformations(Stream stream, ReadOnlySpan<KeyValuePair<uint, uint>> chunkPositions, ImmutableArray<ChunkInformation>.Builder chunkInfoBuilder) {
        Span<uint> buffer = stackalloc uint[2];
        
        foreach ((uint chunkTag, uint position) in chunkPositions) {
            ReadOnlySpan<byte> chunkTagBytes = new ReadOnlySpan<byte>(&chunkTag, sizeof(uint));
            
            if (position >= stream.Length) throw new CorruptedBinaryException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has position surpassed Stream's length.");
            
            stream.Seek(position, SeekOrigin.Begin);

            if (stream.Read(MemoryMarshal.AsBytes(buffer)) < 8) {
                throw new CorruptedBinaryException($"Stream does not contain sufficient data to validate chunk offset and length for chunk '{Encoding.ASCII.GetString(chunkTagBytes)}'.");
            }

            uint validatingChunk = buffer[0];
            
            if (validatingChunk != chunkTag) {
                throw new CorruptedBinaryException($"Expected chunk tag {Encoding.ASCII.GetString(chunkTagBytes)} at position {position}.");
            }
            
            uint contentLength = buffer[1];
            
            if (stream.Position + contentLength > stream.Length) {
                throw new CorruptedBinaryException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has content length surpassed Stream's length.");
            }
            
            chunkInfoBuilder.Add(new(chunkTag, contentLength, stream.Position));
        }
    }
}
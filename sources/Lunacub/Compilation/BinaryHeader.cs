using Caxivitual.Lunacub.Exceptions;
using System.Buffers;
using System.Collections.Immutable;
using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public readonly struct BinaryHeader {
    public readonly ushort MajorVersion;
    public readonly ushort MinorVersion;
    public readonly ImmutableArray<ChunkInformation> Chunks;

    public BinaryHeader(ushort majorVersion, ushort minorVersion, ImmutableArray<ChunkInformation> chunks) {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        Chunks = chunks;
    }

    public bool TryGetChunkInformation(ReadOnlySpan<byte> tag, out ChunkInformation output) {
        if (tag.Length != 4) {
            output = default;
            return false;
        }

        uint tagValue = BinaryPrimitives.ReadUInt32LittleEndian(tag);

        foreach (var info in Chunks) {
            if (info.Tag == tagValue) {
                output = info;
                return true;
            }
        }
        
        output = default;
        return false;
    }
    
    public static BinaryHeader Extract(Stream stream) {
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

    private static unsafe void ValidateChunkInformations(
        Stream stream,
        ReadOnlySpan<KeyValuePair<uint, uint>> chunkPositions,
        ImmutableArray<ChunkInformation>.Builder chunkInfoBuilder)
    {
        Span<uint> buffer = stackalloc uint[2];
        
        foreach ((uint chunkTag, uint position) in chunkPositions) {
            ReadOnlySpan<byte> chunkTagBytes = new ReadOnlySpan<byte>(&chunkTag, sizeof(uint));

            if (position >= stream.Length) {
                string chunkName = Encoding.ASCII.GetString(chunkTagBytes);
                string message = string.Format(ExceptionMessages.ChunkPositionSurpassedStreamLength, chunkName);
                
                throw new CorruptedBinaryException(message);
            }
            
            stream.Seek(position, SeekOrigin.Begin);

            if (stream.Read(MemoryMarshal.AsBytes(buffer)) < 8) {
                string chunkName = Encoding.ASCII.GetString(chunkTagBytes);
                string message = string.Format(ExceptionMessages.FailedToReadChunkHeader, chunkName);
                
                throw new CorruptedBinaryException(message);
            }

            uint validatingChunk = buffer[0];
            
            if (validatingChunk != chunkTag) {
                string chunkName = Encoding.ASCII.GetString(chunkTagBytes);
                string message = string.Format(ExceptionMessages.ExpectedChunkAtPosition, chunkName, position);
                
                throw new CorruptedBinaryException(message);
            }
            
            uint contentLength = buffer[1];
            
            if (stream.Position + contentLength > stream.Length) {
                string chunkName = Encoding.ASCII.GetString(chunkTagBytes);
                string message = string.Format(ExceptionMessages.ChunkContentLengthOverflow, chunkName);
                
                throw new CorruptedBinaryException(message);
            }
            
            chunkInfoBuilder.Add(new(chunkTag, contentLength, stream.Position));
        }
    }
}
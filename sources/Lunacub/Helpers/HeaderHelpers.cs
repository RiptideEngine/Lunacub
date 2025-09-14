using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using System.Buffers;
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Helpers;

public static class HeaderHelpers {
    public static void ExtractChunkPositionalInfo(ReadOnlySpan<ChunkOffset> offsets, Stream stream, Span<ChunkPositionalInformation> outputs) {
        if (offsets.Length > outputs.Length) {
            throw new ArgumentException(ExceptionMessages.MismatchLength_DestinationSpanHasLessElement, nameof(offsets));
        }
        
        if (!stream.CanRead || !stream.CanSeek) {
            throw new ArgumentException(ExceptionMessages.StreamNotReaderOrNotSeekable, nameof(stream));
        }
        
        ref readonly var startOffset = ref offsets[0];
        ref var startOutput = ref outputs[0];

        for (int i = 0; i < offsets.Length; i++) {
            ref readonly var offset = ref Unsafe.Add(ref Unsafe.AsRef(in startOffset), i);
            ref var output = ref Unsafe.Add(ref startOutput, i);

            if (offset.Offset > stream.Length) {
                string message = string.Format(ExceptionMessages.InsufficientStream_OffsetSurpassedStream, offset.Tag.AsHexString);
                throw new ArgumentException(message);
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);

            ChunkLength length = ReadChunkLength(stream);

            if (offset.Tag != length.Tag) {
                string message = string.Format(ExceptionMessages.ExpectedTagAtPosition, offset.Tag.AsHexString, offset.Offset, length.Tag.AsHexString);
                throw new ArgumentException(message);
            }

            if (stream.Position + length.Length > stream.Length) {
                string message = string.Format(ExceptionMessages.InsufficientStream_LengthSurpassedStream, length.Tag.AsHexString);
                throw new ArgumentException(message);
            }

            output = new(offset.Tag, offset.Offset, length.Length, (uint)stream.Position);
        }
    }

    public static Header Extract(Stream stream) {
        if (!stream.CanRead || !stream.CanSeek) {
            throw new ArgumentException(ExceptionMessages.StreamNotReaderOrNotSeekable, nameof(stream));
        }

        ReadMagicNumber(stream);
        (ushort major, ushort minor) = ReadVersionNumbers(stream);
        ImmutableArray<ChunkOffset> chunkOffsets = ReadChunkOffsets(stream);
        
        // TODO: Read extras on other header versions.
        HeaderExtra extras = default;
        
        return new(major, minor, chunkOffsets, extras);
    }

    private static void ReadMagicNumber(Stream stream) {
        Span<byte> span = stackalloc byte[4];
        if (stream.Read(span) < 4) {
            throw new EndOfStreamException(ExceptionMessages.InsufficientStream_MagicNumber);
        }
        
        if (!span.StartsWith(CompilingConstants.MagicIdentifier.AsSpan)) {
            string message = string.Format(ExceptionMessages.ExpectHeaderMagic, Convert.ToHexString(span));
            throw new InvalidDataException(message);
        }
    }

    private static (ushort Major, ushort Minor) ReadVersionNumbers(Stream stream) {
        Span<ushort> span = stackalloc ushort[2];
        if (stream.Read(MemoryMarshal.AsBytes(span)) < 4) {
            throw new EndOfStreamException(ExceptionMessages.InsufficientStream_Version);
        }
        
        // TODO: Detect version dynamically.
        if (span[0] != 1 && span[1] != 0) {
            string message = string.Format(ExceptionMessages.UnsupportedHeaderVersion, span[0], span[1]);
            throw new InvalidDataException(message);
        }

        return (span[0], span[1]);
    }

    private unsafe static ImmutableArray<ChunkOffset> ReadChunkOffsets(Stream stream) {
        int chunkAmount;
        if (stream.Read(new(&chunkAmount, sizeof(int))) < 4) {
            throw new EndOfStreamException(ExceptionMessages.InsufficientStream_ChunkAmount);
        }

        ChunkOffset[] rentedArray = [];
        Span<ChunkOffset> span = chunkAmount <= 16 ? stackalloc ChunkOffset[chunkAmount] : (rentedArray = ArrayPool<ChunkOffset>.Shared.Rent(chunkAmount));

        try {
            if (stream.Read(MemoryMarshal.AsBytes(span[..chunkAmount])) < sizeof(ChunkOffset) * chunkAmount) {
                throw new EndOfStreamException(ExceptionMessages.InsufficientStream_ChunkOffsets);
            }

            return [..span[..chunkAmount]];
        } finally {
            ArrayPool<ChunkOffset>.Shared.Return(rentedArray);
        }
    }

    private unsafe static ChunkLength ReadChunkLength(Stream stream) {
        ChunkLength length;

        if (stream.Read(new(&length, sizeof(ChunkLength))) < sizeof(ChunkLength)) {
            throw new EndOfStreamException(ExceptionMessages.InsufficientStream_ChunkLength);
        }

        return length;
    }
    
    public static bool TryGet(this ReadOnlyMemory<ChunkOffset> memory, Tag tag, out ChunkOffset output) {
        return TryGet(memory.Span, tag, out output);
    }

    public static bool TryGet(this ReadOnlySpan<ChunkOffset> span, Tag tag, out ChunkOffset output) {
        foreach (var information in span) {
            if (information.Tag == tag) {
                output = information;
                return true;
            }
        }
        
        output = default;
        return false;
    }
    
    public static bool TryGet(this ReadOnlyMemory<ChunkLength> memory, Tag tag, out ChunkLength output) {
        return TryGet(memory.Span, tag, out output);
    }

    public static bool TryGet(this ReadOnlySpan<ChunkLength> span, Tag tag, out ChunkLength output) {
        foreach (var information in span) {
            if (information.Tag == tag) {
                output = information;
                return true;
            }
        }
        
        output = default;
        return false;
    }
    
    public static bool TryGet(this ReadOnlyMemory<ChunkPositionalInformation> memory, Tag tag, out ChunkPositionalInformation output) {
        return TryGet(memory.Span, tag, out output);
    }

    public static bool TryGet(this ReadOnlySpan<ChunkPositionalInformation> span, Tag tag, out ChunkPositionalInformation output) {
        foreach (var information in span) {
            if (information.Tag == tag) {
                output = information;
                return true;
            }
        }
        
        output = default;
        return false;
    }
}
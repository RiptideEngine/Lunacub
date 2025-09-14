using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Compilation;

public readonly struct Header {
    public readonly ushort MajorVersion;
    public readonly ushort MinorVersion;
    public readonly ImmutableArray<ChunkOffset> ChunkOffsets;
    public readonly HeaderExtra Extras;

    public Header(ushort majorVersion, ushort minorVersion, ImmutableArray<ChunkOffset> chunkOffsets, HeaderExtra extras) {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        ChunkOffsets = chunkOffsets;
        Extras = extras;
    }
}
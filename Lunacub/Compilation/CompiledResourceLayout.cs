using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Compilation;

public readonly struct CompiledResourceLayout {
    public readonly ushort MajorVersion;
    public readonly ushort MinorVersion;
    public readonly ImmutableArray<ChunkInformation> Chunks;

    internal CompiledResourceLayout(ushort majorVersion, ushort minorVersion, ImmutableArray<ChunkInformation> chunks) {
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
}
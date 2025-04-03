using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Compilation;

public struct CompiledResourceLayout {
    public ushort MajorVersion;
    public ushort MinorVersion;
    public ImmutableArray<ChunkInformation> Chunks;

    public readonly bool TryGetChunkInformation(ReadOnlySpan<byte> tag, out ChunkInformation output) {
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
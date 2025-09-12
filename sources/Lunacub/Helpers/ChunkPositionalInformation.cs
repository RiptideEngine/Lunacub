using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Helpers;

public readonly record struct ChunkPositionalInformation(Tag Tag, uint Offset, uint Length, uint ContentOffset);
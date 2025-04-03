namespace Caxivitual.Lunacub.Compilation;

public readonly record struct ChunkInformation(uint Tag, uint Length, long ContentOffset);
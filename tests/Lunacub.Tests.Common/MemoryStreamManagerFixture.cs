using Microsoft.IO;

namespace Caxivitual.Lunacub.Tests;

public sealed class MemoryStreamManagerFixture {
    public RecyclableMemoryStreamManager Manager { get; } = new();
}
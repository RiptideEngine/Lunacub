using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemorySourceProvider : SourceProvider {
    public Dictionary<string, Element> Resources { get; } = [];

    protected override Stream? CreateStreamCore(string address) {
        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(Resources[address].Content)!, false);
    }

    public override DateTime GetLastWriteTime(string address) {
        return Resources[address].LastWriteTime;
    }

    public static Element AsUtf8(ReadOnlySpan<char> content, DateTime lastWriteTime) {
        byte[] buffer = new byte[Encoding.UTF8.GetByteCount(content)];
        Encoding.UTF8.GetBytes(content, buffer);
        
        return new(ImmutableCollectionsMarshal.AsImmutableArray(buffer), lastWriteTime);
    }

    public readonly record struct Element(ImmutableArray<byte> Content, DateTime LastWriteTime);
}
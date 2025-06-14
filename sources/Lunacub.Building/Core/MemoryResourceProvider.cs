using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryResourceProvider : ResourceProvider {
    private readonly ImmutableArray<byte> _content;

    public override DateTime LastWriteTime { get; }

    public MemoryResourceProvider(ImmutableArray<byte> content, DateTime lastWriteTime) {
        _content = content.IsDefault ? ImmutableArray<byte>.Empty : content;
        LastWriteTime = lastWriteTime;
    }

    public MemoryResourceProvider(ReadOnlySpan<byte> content, DateTime lastWriteTime) {
        _content = [..content];
        LastWriteTime = lastWriteTime;
    }
    
    public override Stream GetStream() => new MemoryStream(ImmutableCollectionsMarshal.AsArray(_content)!, false);

    public static MemoryResourceProvider Create(string content, Encoding encoding, DateTime lastWriteTime) {
        return new(encoding.GetBytes(content), lastWriteTime);
    }

    public static MemoryResourceProvider AsUtf8(string content, DateTime lastWriteTime) {
        return Create(content, Encoding.UTF8, lastWriteTime);
    }
}
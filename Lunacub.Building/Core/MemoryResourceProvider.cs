using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryResourceProvider : ResourceProvider {
    private readonly ImmutableArray<byte> _content;
    private readonly DateTime _lastWriteTime;

    public MemoryResourceProvider(ImmutableArray<byte> content, DateTime lastWriteTime) {
        _content = content.IsDefault ? ImmutableArray<byte>.Empty : content;
        _lastWriteTime = lastWriteTime;
    }
    
    public override DateTime GetLastWriteTime() => _lastWriteTime;
    public override Stream GetStream() => new MemoryStream(ImmutableCollectionsMarshal.AsArray(_content)!, false);
}
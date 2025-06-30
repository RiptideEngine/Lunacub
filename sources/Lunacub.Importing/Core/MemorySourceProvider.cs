using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemorySourceProvider : SourceProvider {
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; } = [];

    protected override Stream? CreateStreamCore(ResourceID resourceId) {
        if (!Resources.TryGetValue(resourceId, out var buffer)) return null;
    
        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
}
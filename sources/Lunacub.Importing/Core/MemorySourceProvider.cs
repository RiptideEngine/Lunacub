using System.Collections.Immutable;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemorySourceProvider : ImportSourceProvider {
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; } = [];

    protected override Stream? CreateStreamCore(ResourceID id) {
        if (!Resources.TryGetValue(id, out var buffer)) return null;
    
        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
}
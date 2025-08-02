using System.Collections.Immutable;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemorySourceProvider : ImportSourceProvider {
    public Dictionary<ResourceAddress, ImmutableArray<byte>> Resources { get; } = [];

    protected override Stream? CreateStreamCore(ResourceAddress address) {
        if (!Resources.TryGetValue(address, out var buffer)) return null;
    
        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
}
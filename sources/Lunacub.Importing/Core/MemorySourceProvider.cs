using System.Collections.Immutable;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

public sealed class MemorySourceProvider : ImportSourceProvider {
    public Dictionary<ResourceID, ImmutableArray<byte>> Resources { get; } = [];

    protected override Stream? CreateStreamCore(ResourceID resourceId) {
        if (!Resources.TryGetValue(resourceId, out var buffer)) return null;
    
        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(buffer) ?? [], false);
    }
    
    protected override Stream? CreateStreamCore(string address) {
        if (ResourceID.TryParse(Path.GetFileNameWithoutExtension(address), NumberStyles.HexNumber, null, out var resourceId)) {
            return CreateStreamCore(resourceId);
        }

        return null;
    }
}
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryResourceLibrary : BuildResourceLibrary {
    public Dictionary<string, Element> Resources { get; } = [];
    
    protected override Stream? CreateResourceStreamCore(ResourceID resourceId, BuildingResource element) {
        if (!Resources.TryGetValue(resourceId, out var memoryElement)) return null;

        return new MemoryStream(ImmutableCollectionsMarshal.AsArray(memoryElement.Content) ?? [], false);
    }
    
    protected override DateTime GetResourceLastWriteTimeCore(ResourceID resourceId, BuildingResource resource) {
        if (!Resources.TryGetValue(resourceId, out var memoryElement)) return default;

        return memoryElement.LastWriteTime;
    }

    public static Element AsUtf8(ReadOnlySpan<char> content, DateTime lastWriteTime) {
        byte[] buffer = new byte[Encoding.UTF8.GetByteCount(content)];
        Encoding.UTF8.GetBytes(content, buffer);
        
        return new(ImmutableCollectionsMarshal.AsImmutableArray(buffer), lastWriteTime);
    }

    public readonly record struct Element(ImmutableArray<byte> Content, DateTime LastWriteTime);
}
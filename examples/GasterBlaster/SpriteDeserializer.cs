// ReSharper disable AccessToDisposedClosure

using Caxivitual.Lunacub.Extensions;
using Caxivitual.Lunacub.Importing;
using System.Text;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed class SpriteDeserializer : Deserializer<Sprite> {
    protected override Task<Sprite> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using BinaryReader reader = new(dataStream, Encoding.UTF8, leaveOpen: true);

        string name = reader.ReadString();
        
        List<Subsprite> subsprites = new(reader.ReadInt32());
        
        for (int i = 0, c = subsprites.Capacity; i < c; i++) {
            subsprites.Add(new(reader.ReadString(), new(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32())));
        }

        ResourceID textureId = reader.ReadResourceID();

        Sprite sprite = new() {
            Name = name,
        };
        sprite.Subsprites.AddRange(subsprites);
        
        context.RequestReference<Texture2D>(1, textureId);

        return Task.FromResult(sprite);
    }

    protected override void ResolveReferences(Sprite instance, DeserializationContext context) {
        instance.Texture = context.GetReference<Texture2D>(1) ?? throw new ArgumentException("Null texture.");
    }
}
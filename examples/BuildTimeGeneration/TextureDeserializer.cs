using System.Text;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public sealed class TextureDeserializer : Deserializer<Texture> {
    protected override Task<Texture> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, leaveOpen: true);
        
        string textureName = reader.ReadString();
        Sprite[] sprites = new Sprite[reader.ReadInt32()];

        for (int i = 0; i < sprites.Length; i++) {
            string spriteName = reader.ReadString();
            Rectangle rect = reader.ReadReinterpret<Rectangle>();
            
            sprites[i] = new(spriteName, rect);
        }
        
        return Task.FromResult(new Texture(textureName, sprites));
    }
}
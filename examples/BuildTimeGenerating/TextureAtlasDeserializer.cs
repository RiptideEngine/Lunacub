namespace Caxivitual.Lunacub.Examples.BuildTimeGenerating;

public sealed class TextureAtlasDeserializer : Deserializer<TextureAtlas> {
    protected override Task<TextureAtlas> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
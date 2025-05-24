namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public sealed class TextureAtlasDeserializer : Deserializer<TextureAtlas> {
    protected override Task<TextureAtlas> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
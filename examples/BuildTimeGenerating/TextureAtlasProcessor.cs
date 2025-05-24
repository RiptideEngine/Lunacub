namespace Caxivitual.Lunacub.Examples.BuildTimeGenerating;

public sealed class TextureAtlasProcessor : Processor<TextureAtlasDTO, TextureAtlasDTO> {
    protected override TextureAtlasDTO Process(TextureAtlasDTO input, ProcessingContext context) {
        // TODO: Generate combined texture data.
        return input;
    }
}
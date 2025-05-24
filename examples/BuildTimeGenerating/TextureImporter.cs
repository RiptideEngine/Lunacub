using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.BuildTimeGenerating;

public sealed class TextureImporter : Importer<Texture> {
    protected override Texture Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<Texture>(stream)!;
    }
}
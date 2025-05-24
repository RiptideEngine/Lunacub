using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public sealed class TextureDTO : ContentRepresentation {
    public string Name { get; }
    public Sprite[] Sprites { get; }

    [JsonConstructor]
    public TextureDTO(string name, Sprite[] sprites) {
        Name = name;
        Sprites = sprites;
    }
}

public sealed class TextureImporter : Importer<TextureDTO> {
    protected override TextureDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<TextureDTO>(stream)!;
    }
}
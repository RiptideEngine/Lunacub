using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Examples.BuildTimeGenerating;

public sealed class TextureAtlasDTO : ContentRepresentation {
    public string Name { get; }
    public ResourceID[] Textures { get; }

    [JsonConstructor]
    public TextureAtlasDTO(string name, ResourceID[] textures) {
        Name = name;
        Textures = textures;
    }
}

public sealed class TextureAtlasImporter : Importer<TextureAtlasDTO> {
    protected override TextureAtlasDTO Import(Stream stream, ImportingContext context) {
        var output = JsonSerializer.Deserialize<TextureAtlasDTO>(stream)!;

        foreach (var textureID in output.Textures) {
            context.AddReference(textureID);
        }
        
        return output;
    }
}
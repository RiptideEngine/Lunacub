using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

// Make it a ContentRepresentation to not have to create an exact TextureDTO class in this demo.
public sealed class Texture : ContentRepresentation {
    public string Name { get; }
    public Sprite[] Sprites { get; }
    
    [JsonConstructor]
    public Texture(string name, Sprite[] sprites) {
        Name = name;
        Sprites = sprites;
    }
}
namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

// Typically, Atlas should be built from collection of Sprites, but use Textures for demo purpose.
public sealed class TextureAtlas {
    public string Name { get; }
    public Texture[] Textures { get; }

    public TextureAtlas(string name, Texture[] textures) {
        Name = name;
        Textures = textures;
    }
}
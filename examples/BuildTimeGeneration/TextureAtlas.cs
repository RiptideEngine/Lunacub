namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

// Typically, Atlas should be built from collection of Sprites, but use Textures for demo purpose.
public sealed class TextureAtlas {
    public string Name { get; }
    public Texture AtlasTexture { get; }
    public Texture[] Textures { get; }

    public TextureAtlas(string name, Texture atlasTexture, Texture[] textures) {
        Name = name;
        AtlasTexture = atlasTexture;
        Textures = textures;
    }
}
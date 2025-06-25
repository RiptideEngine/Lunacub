using Silk.NET.Maths;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed class Sprite {
    public string Name { get; private set; }
    public Texture2D Texture { get; private set; }
    public Rectangle<uint> Region { get; private set; }

    public Sprite(string name, Texture2D texture, Rectangle<uint> region) {
        Name = name;
        Texture = texture;
        Region = region;
    }
}
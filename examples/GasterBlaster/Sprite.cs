namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed class Sprite {
    public string Name { get; set; }
    public Texture2D Texture { get; set; }
    public List<Subsprite> Subsprites { get; }

    public Sprite() {
        Name = string.Empty;
        Subsprites = [];
        Texture = null!;
    }
}
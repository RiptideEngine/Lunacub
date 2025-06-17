using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public sealed class Texture2DDTO : ContentRepresentation {
    public Image<Rgba32> Image { get; private set; }

    public Texture2DDTO(Image<Rgba32> image) {
        Image = image;
    }

    protected override void DisposeImpl(bool disposing) {
        base.DisposeImpl(disposing);

        if (disposing) {
            Image.Dispose();
            Image = null!;
        }
    }
}

[AutoTimestampVersion("yyyyMMdd_HHmmss")]
public sealed partial class Texture2DImporter : Importer<Texture2DDTO> {
    protected override Texture2DDTO Import(Stream resourceStream, ImportingContext context) {
        return new(Image.Load<Rgba32>(resourceStream));
    }
}
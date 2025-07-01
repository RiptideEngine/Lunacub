using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed class SpriteDTO : ContentRepresentation {
    public string Name { get; set; }
    public ResourceID TextureId { get; set; }
    public List<Subsprite> Subsprites { get; set; }
}

[AutoTimestampVersion("yyyyMMdd_HHmmss")]
public sealed partial class SpriteImporter : Importer<SpriteDTO> {
    public override ImporterFlags Flags => ImporterFlags.NoDependency;

    protected override SpriteDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<SpriteDTO>(sourceStreams.PrimaryStream!, new JsonSerializerOptions {
            IncludeFields = true,
        })!;
    }
}
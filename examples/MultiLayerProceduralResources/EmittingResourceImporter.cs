using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace MultiLayerProceduralResources;

[AutoTimestampVersion("yyyyMMddHHmmss")]
public sealed partial class EmittingResourceImporter : Importer<EmittingResourceDTO> {
    protected override EmittingResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<EmittingResourceDTO>(sourceStreams.PrimaryStream!)!;
    }
}
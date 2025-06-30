using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class EmittableResourceImporter : Importer<EmittableResourceDTO> {
    protected override EmittableResourceDTO Import(SourceStreams streams, ImportingContext context) {
        return JsonSerializer.Deserialize<EmittableResourceDTO>(streams.PrimaryStream!)!;
    }
}
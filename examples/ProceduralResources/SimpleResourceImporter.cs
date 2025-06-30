using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(sourceStreams.PrimaryStream!)!;
    }
}
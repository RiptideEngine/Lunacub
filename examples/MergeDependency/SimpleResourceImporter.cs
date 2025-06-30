using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(SourceStreams streams, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(streams.PrimaryStream!)!;
    }
}
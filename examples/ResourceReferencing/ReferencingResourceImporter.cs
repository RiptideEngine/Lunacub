using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class ReferencingResourceImporter : Importer<ReferencingResourceDTO> {
    public override ImporterFlags Flags => ImporterFlags.NoDependency;

    protected override ReferencingResourceDTO Import(SourceStreams streams, ImportingContext context) {
        return JsonSerializer.Deserialize<ReferencingResourceDTO>(streams.PrimaryStream!)!;
    }
}
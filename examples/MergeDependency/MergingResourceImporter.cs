using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class MergingResourceImporter : Importer<MergingResourceDTO> {
    public override IReadOnlyCollection<ResourceID> ExtractDependencies(SourceStreams sourceStream) {
        return JsonSerializer.Deserialize<MergingResourceDTO>(sourceStream.PrimaryStream!)!.Dependencies;
    }

    protected override MergingResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<MergingResourceDTO>(sourceStreams.PrimaryStream!)!;
    }
}
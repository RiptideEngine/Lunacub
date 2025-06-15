using Caxivitual.Lunacub.Building.Attributes;
using System.Text;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class MergingResourceImporter : Importer<MergingResourceDTO> {
    public override IReadOnlyCollection<ResourceID> ExtractDependencies(Stream stream) {
        // Typically you will deserialize into a surrogate type that only contains the dependency informations,
        // but we will just deserialize as the DTO as a quick and dirty way.
        return JsonSerializer.Deserialize<MergingResourceDTO>(stream)!.Dependencies;
    }

    protected override MergingResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<MergingResourceDTO>(resourceStream)!;
    }
}
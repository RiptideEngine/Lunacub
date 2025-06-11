using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed partial class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(stream)!;
    }
}
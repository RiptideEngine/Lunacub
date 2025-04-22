using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.SimpleResourceImporting;

public sealed class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override SimpleResourceDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(stream, _jsonOptions)!;
    }
}
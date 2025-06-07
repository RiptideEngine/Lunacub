using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ReferenceImporting;

public sealed partial class ReferenceResourceImporter : Importer<ReferenceResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override ReferenceResourceDTO Import(Stream stream, ImportingContext context) {
        var dto = JsonSerializer.Deserialize<ReferenceResourceDTO>(stream, _jsonOptions)!;
        
        context.AddReference(dto.Reference);

        return dto;
    }
}
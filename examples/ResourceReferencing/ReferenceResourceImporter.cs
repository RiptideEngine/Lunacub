using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public sealed partial class ReferenceResourceImporter : Importer<ReferenceResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override ReferenceResourceDTO Import(Stream resourceStream, ImportingContext context) {
        var dto = JsonSerializer.Deserialize<ReferenceResourceDTO>(resourceStream, _jsonOptions)!;
        
        context.AddReference(dto.Reference);

        return dto;
    }
}
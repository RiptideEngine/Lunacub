using System.Text.Json;

namespace MultiLayerProceduralResources;

public sealed partial class EmittingResourceImporter : Importer<EmittingResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override EmittingResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<EmittingResourceDTO>(resourceStream, _jsonOptions)!;
    }
}
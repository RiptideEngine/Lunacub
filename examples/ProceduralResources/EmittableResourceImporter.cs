using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class EmittableResourceImporter : Importer<EmittableResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override EmittableResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<EmittableResourceDTO>(resourceStream, _jsonOptions)!;
    }
}
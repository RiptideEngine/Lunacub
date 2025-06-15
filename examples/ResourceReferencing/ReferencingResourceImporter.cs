using Caxivitual.Lunacub.Building.Attributes;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class ReferencingResourceImporter : Importer<ReferencingResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override ReferencingResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<ReferencingResourceDTO>(resourceStream, _jsonOptions)!;
    }
}
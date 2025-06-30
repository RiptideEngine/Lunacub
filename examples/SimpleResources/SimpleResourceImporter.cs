using Caxivitual.Lunacub.Building.Attributes;
using System.Diagnostics;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.SimpleResources;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    public override ImporterFlags Flags => ImporterFlags.NoDependency;

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };

    protected override SimpleResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        Debug.Assert(sourceStreams.PrimaryStream != null);
        
        return JsonSerializer.Deserialize<SimpleResourceDTO>(sourceStreams.PrimaryStream, _jsonOptions)!;
    }
}
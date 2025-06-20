﻿using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed partial class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default) {
        IncludeFields = true,
    };
    
    protected override SimpleResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(resourceStream, _jsonOptions)!;
    }
}
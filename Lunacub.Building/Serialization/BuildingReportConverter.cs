namespace Caxivitual.Lunacub.Building.Serialization;

internal sealed class BuildingReportConverter : JsonConverter<BuildingReport> {
    public override BuildingReport Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }
            
        BuildingReport output = default;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return output;
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case "SourceLastWriteTime":
                    output.SourceLastWriteTime = reader.GetDateTime();
                    break;
                    
                case "Dependencies":
                    output.Dependencies = JsonSerializer.Deserialize<HashSet<ResourceID>>(ref reader, options) ?? [];
                    break;
                
                case "Options":
                    output.Options = JsonSerializer.Deserialize<BuildingOptions>(ref reader, options);
                    break;
            }
        }

        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, BuildingReport report, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WriteString("SourceLastWriteTime", report.SourceLastWriteTime);
        
        writer.WritePropertyName("Dependencies");
        JsonSerializer.Serialize(writer, report.Dependencies, options);
        
        writer.WritePropertyName("Options");
        JsonSerializer.Serialize(writer, report.Options, options);
        
        writer.WriteEndObject();
    }
}
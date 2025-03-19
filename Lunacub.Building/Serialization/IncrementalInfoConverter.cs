namespace Caxivitual.Lunacub.Building.Serialization;

internal sealed class IncrementalInfoConverter : JsonConverter<IncrementalInfo> {
    public override IncrementalInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        DateTime sourceLastWriteTime = default;
        BuildingOptions buildingOptions = default;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(sourceLastWriteTime, buildingOptions);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case "SourceLastWriteTime":
                    sourceLastWriteTime= reader.GetDateTime();
                    break;
                    
                case "Options":
                    buildingOptions = JsonSerializer.Deserialize<BuildingOptions>(ref reader, options);
                    break;
            }
        }

        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, IncrementalInfo report, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WriteString("SourceLastWriteTime", report.SourceLastWriteTime);
        
        writer.WritePropertyName("Options");
        JsonSerializer.Serialize(writer, report.Options, options);
        
        writer.WriteEndObject();
    }
}
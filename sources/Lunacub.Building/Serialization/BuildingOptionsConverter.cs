using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building.Serialization;

internal sealed class BuildingOptionsConverter : JsonConverter<BuildingOptions> {
    public override BuildingOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }
        
        string importerName = string.Empty, processorName = string.Empty;
        IImportOptions? buildOptions = null;
        
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(importerName, processorName, buildOptions);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case nameof(BuildingOptions.ImporterName):
                    importerName = reader.GetString() ?? string.Empty;
                    break;
                    
                case nameof(BuildingOptions.ProcessorName):
                    processorName = reader.GetString() ?? string.Empty;
                    break;
                
                case nameof(BuildingOptions.Options):
                    buildOptions = JsonSerializer.Deserialize<IImportOptions>(ref reader, options);
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, BuildingOptions value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        {
            writer.WriteString(nameof(BuildingOptions.ImporterName), value.ImporterName);
            writer.WriteString(nameof(BuildingOptions.ProcessorName), value.ProcessorName);

            writer.WritePropertyName(nameof(BuildingOptions.Options));
            JsonSerializer.Serialize(writer, value.Options, options);
        }
        writer.WriteEndObject();
    }
}
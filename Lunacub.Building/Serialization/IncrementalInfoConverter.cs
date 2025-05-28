using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building.Serialization;

[ExcludeFromCodeCoverage]
internal sealed class IncrementalInfoConverter : JsonConverter<IncrementalInfo> {
    public override IncrementalInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        DateTime sourceLastWriteTime = default;
        BuildingOptions buildingOptions = default;
        IReadOnlySet<ResourceID> dependencies = FrozenSet<ResourceID>.Empty;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(sourceLastWriteTime, buildingOptions, dependencies);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name.");
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
                
                case "Dependencies":
                    dependencies = JsonSerializer.Deserialize<HashSet<ResourceID>>(ref reader, options) is { } set ? set.ToFrozenSet() : FrozenSet<ResourceID>.Empty;
                    break;
            }
        }

        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, IncrementalInfo info, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WriteString("SourceLastWriteTime", info.SourceLastWriteTime);
        
        writer.WritePropertyName("Options");
        JsonSerializer.Serialize(writer, info.Options, options);
        
        writer.WritePropertyName("Dependencies");
        JsonSerializer.Serialize(writer, info.Dependencies, options);
        
        writer.WriteEndObject();
    }
}
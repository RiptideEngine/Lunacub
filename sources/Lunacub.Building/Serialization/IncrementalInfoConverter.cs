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
        ComponentVersions componentVersions = default;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(sourceLastWriteTime, buildingOptions, dependencies, componentVersions);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name.");
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case nameof(IncrementalInfo.SourceLastWriteTime):
                    sourceLastWriteTime= reader.GetDateTime();
                    break;
                    
                case nameof(IncrementalInfo.Options):
                    buildingOptions = JsonSerializer.Deserialize<BuildingOptions>(ref reader, options);
                    break;
                
                case nameof(IncrementalInfo.Dependencies):
                    dependencies = JsonSerializer.Deserialize<HashSet<ResourceID>>(ref reader, options) is { } set ? set.ToFrozenSet() : FrozenSet<ResourceID>.Empty;
                    break;
                
                case nameof(IncrementalInfo.ComponentVersions):
                    componentVersions = JsonSerializer.Deserialize<ComponentVersions>(ref reader, options);
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, IncrementalInfo info, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WriteString(nameof(IncrementalInfo.SourceLastWriteTime), info.SourceLastWriteTime);
        
        writer.WritePropertyName(nameof(IncrementalInfo.Options));
        JsonSerializer.Serialize(writer, info.Options, options);
        
        writer.WritePropertyName(nameof(IncrementalInfo.Dependencies));
        JsonSerializer.Serialize(writer, info.Dependencies, options);
        
        writer.WritePropertyName(nameof(IncrementalInfo.ComponentVersions));
        JsonSerializer.Serialize(writer, info.ComponentVersions, options);
        
        writer.WriteEndObject();
    }
}
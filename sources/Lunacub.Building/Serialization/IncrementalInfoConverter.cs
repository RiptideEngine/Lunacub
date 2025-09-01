using Caxivitual.Lunacub.Building.Incremental;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building.Serialization;

[ExcludeFromCodeCoverage]
internal sealed class IncrementalInfoConverter : JsonConverter<IncrementalInfo> {
    public override IncrementalInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        SourcesInfo sourcesInfo = default;
        BuildingOptions buildingOptions = default;
        IReadOnlySet<ResourceAddress> dependencies = FrozenSet<ResourceAddress>.Empty;
        ComponentVersions componentVersions = default;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(sourcesInfo, buildingOptions, dependencies, componentVersions);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name.");
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case nameof(IncrementalInfo.SourcesInfo):
                    sourcesInfo = JsonSerializer.Deserialize<SourcesInfo>(ref reader, options);
                    break;
                    
                case nameof(IncrementalInfo.Options):
                    buildingOptions = JsonSerializer.Deserialize<BuildingOptions>(ref reader, options);
                    break;
                
                case nameof(IncrementalInfo.DependencyAddresses):
                    dependencies = JsonSerializer.Deserialize<HashSet<ResourceAddress>>(ref reader, options) is { } set ?
                        set.ToFrozenSet() :
                        FrozenSet<ResourceAddress>.Empty;
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
        
        writer.WritePropertyName(nameof(IncrementalInfo.SourcesInfo));
        JsonSerializer.Serialize(writer, info.SourcesInfo, options);
        
        writer.WritePropertyName(nameof(IncrementalInfo.Options));
        JsonSerializer.Serialize(writer, info.Options, options);
        
        writer.WritePropertyName(nameof(IncrementalInfo.DependencyAddresses));
        JsonSerializer.Serialize(writer, info.DependencyAddresses, options);
        
        writer.WritePropertyName(nameof(IncrementalInfo.ComponentVersions));
        JsonSerializer.Serialize(writer, info.ComponentVersions, options);
        
        writer.WriteEndObject();
    }
}
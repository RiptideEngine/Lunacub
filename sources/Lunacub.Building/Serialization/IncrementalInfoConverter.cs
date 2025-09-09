using Caxivitual.Lunacub.Building.Incremental;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building.Serialization;

[ExcludeFromCodeCoverage]
internal sealed class IncrementalInfoConverter : JsonConverter<BuildCache> {
    public override BuildCache Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
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
                case nameof(BuildCache.SourcesInfo):
                    sourcesInfo = JsonSerializer.Deserialize<SourcesInfo>(ref reader, options);
                    break;
                    
                case nameof(BuildCache.Options):
                    buildingOptions = JsonSerializer.Deserialize<BuildingOptions>(ref reader, options);
                    break;
                
                case nameof(BuildCache.DependencyAddresses):
                    dependencies = JsonSerializer.Deserialize<HashSet<ResourceAddress>>(ref reader, options) is { } set ?
                        set.ToFrozenSet() :
                        FrozenSet<ResourceAddress>.Empty;
                    break;
                
                case nameof(BuildCache.ComponentVersions):
                    componentVersions = JsonSerializer.Deserialize<ComponentVersions>(ref reader, options);
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, BuildCache info, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(BuildCache.SourcesInfo));
        JsonSerializer.Serialize(writer, info.SourcesInfo, options);
        
        writer.WritePropertyName(nameof(BuildCache.Options));
        JsonSerializer.Serialize(writer, info.Options, options);
        
        writer.WritePropertyName(nameof(BuildCache.DependencyAddresses));
        JsonSerializer.Serialize(writer, info.DependencyAddresses, options);
        
        writer.WritePropertyName(nameof(BuildCache.ComponentVersions));
        JsonSerializer.Serialize(writer, info.ComponentVersions, options);
        
        writer.WriteEndObject();
    }
}
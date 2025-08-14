using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Serialization;

internal sealed class ResourceAddressConverter : JsonConverter<ResourceAddress> {
    public override ResourceAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        LibraryID libraryId = default;
        ResourceID resourceId = default;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(libraryId, resourceId);
            }
                
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException();
            }

            switch (reader.GetString()) {
                case nameof(ResourceAddress.LibraryId):
                    libraryId = JsonSerializer.Deserialize<LibraryID>(ref reader, options);
                    break;
                
                case nameof(ResourceAddress.ResourceId):
                    resourceId = JsonSerializer.Deserialize<ResourceID>(ref reader, options);
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }
            
        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, ResourceAddress value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(ResourceAddress.LibraryId));
        ((JsonConverter<LibraryID>)options.GetConverter(typeof(LibraryID))).Write(writer, value.LibraryId, options);
        
        writer.WritePropertyName(nameof(ResourceAddress.ResourceId));
        ((JsonConverter<ResourceID>)options.GetConverter(typeof(ResourceID))).Write(writer, value.ResourceId, options);
        
        writer.WriteEndObject();
    }
}
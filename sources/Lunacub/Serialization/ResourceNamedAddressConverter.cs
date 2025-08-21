using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Serialization;

internal sealed class ResourceNamedAddressConverter : JsonConverter<NamedResourceAddress> {
    public override NamedResourceAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        LibraryID libraryId = default;
        string name = string.Empty;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(libraryId, name);
            }
                
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException();
            }

            switch (reader.GetString()) {
                case nameof(NamedResourceAddress.LibraryId):
                    libraryId = JsonSerializer.Deserialize<LibraryID>(ref reader, options);
                    break;
                
                case nameof(NamedResourceAddress.Name):
                    name = reader.GetString()!;
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }
            
        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, NamedResourceAddress value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(NamedResourceAddress.LibraryId));
        ((JsonConverter<LibraryID>)options.GetConverter(typeof(LibraryID))).Write(writer, value.LibraryId, options);
        
        writer.WriteString(nameof(NamedResourceAddress.Name), value.Name);
        
        writer.WriteEndObject();
    }
}
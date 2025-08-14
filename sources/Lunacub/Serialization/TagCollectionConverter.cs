using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Serialization;

internal sealed class TagCollectionConverter : JsonConverter<TagCollection> {
    public override TagCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException("Expected start array.");
        }

        TagCollection.Builder builder = TagCollection.CreateBuilder();

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndArray) return builder.ToCollection();

            if (reader.GetString() is { } tag && TagCollection.IsValidTag(tag)) {
                builder.Add(tag);
            }
        }

        throw new JsonException("Unexpected end of json.");
    }

    public override void Write(Utf8JsonWriter writer, TagCollection value, JsonSerializerOptions options) {
        writer.WriteStartArray();

        foreach (var tag in value) {
            writer.WriteStringValue(tag);
        }
        
        writer.WriteEndArray();
    }
}
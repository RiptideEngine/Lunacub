namespace Caxivitual.Lunacub.Building.Serialization;

internal sealed class SourceAddressesConverter : JsonConverter<SourceAddresses> {
    public override SourceAddresses Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException();
        }

        string primary = string.Empty;
        IReadOnlyDictionary<string, string> secondaries = FrozenDictionary<string, string>.Empty;
            
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new(primary, secondaries);
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name.");
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName) {
                case nameof(SourceAddresses.Primary):
                    primary = reader.GetString() ?? string.Empty;
                    break;
                    
                case nameof(SourceAddresses.Secondaries):
                    secondaries = JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(ref reader, options) ?? FrozenDictionary<string, string>.Empty;
                    break;
                
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, SourceAddresses value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        {
            writer.WriteString(nameof(SourceAddresses.Primary), value.Primary);

            if (value.Secondaries is { } secondaries && secondaries.Count != 0) {
                writer.WriteStartObject(nameof(SourceAddresses.Secondaries));
                {
                    foreach ((var k, var v) in secondaries) {
                        writer.WriteString(k, v);
                    }
                }
                writer.WriteEndObject();
            }
        }
        writer.WriteEndObject();
    }
}
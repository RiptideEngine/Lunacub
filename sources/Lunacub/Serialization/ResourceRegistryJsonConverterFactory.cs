using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Serialization;

internal sealed class ResourceRegistryJsonConverterFactory : JsonConverterFactory {
    private static readonly Type ResourceIDType = typeof(ResourceID);
    private static readonly Type GenericResourceRegistryType = typeof(ResourceRegistry<>);
    
    public override bool CanConvert(Type typeToConvert) {
        return typeToConvert.GetGenericTypeDefinition() == GenericResourceRegistryType;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        Type elementType = typeToConvert.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(InnerConverter<>).MakeGenericType(elementType), BindingFlags.Instance | BindingFlags.Public, null, [options], null)!;

        return converter;
    }

    private sealed class InnerConverter<T> : JsonConverter<ResourceRegistry<T>> {
        private readonly JsonConverter<ResourceID> _resourceIdConverter;
        private readonly JsonConverter<ResourceRegistry<T>.Element> _elementConverter;

        public InnerConverter(JsonSerializerOptions options) {
            _resourceIdConverter = (JsonConverter<ResourceID>)options.GetConverter(typeof(ResourceID));
            _elementConverter = (JsonConverter<ResourceRegistry<T>.Element>)options.GetConverter(typeof(ResourceRegistry<T>.Element));
        }
        
        public override ResourceRegistry<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException();
            }

            ResourceRegistry<T> registry = [];

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    return registry;
                }
                
                if (reader.TokenType != JsonTokenType.PropertyName) {
                    throw new JsonException();
                }
                
                ResourceID resourceID = _resourceIdConverter.Read(ref reader, ResourceIDType, options);

                reader.Read();

                if (_elementConverter.Read(ref reader, typeof(ResourceRegistry<T>.Element), options) is { } element) {
                    registry[resourceID] = element;
                }
            }
            
            throw new JsonException();
        }
        
        public override void Write(Utf8JsonWriter writer, ResourceRegistry<T> value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            
            foreach ((var resourceId, var element) in value) {
                _resourceIdConverter.WriteAsPropertyName(writer, resourceId, options);
                _elementConverter.Write(writer, element, options);
            }
            
            writer.WriteEndObject();
        }
    }
}
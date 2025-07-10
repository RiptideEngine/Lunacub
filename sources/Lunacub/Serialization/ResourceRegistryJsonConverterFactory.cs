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

        Type converterType = typeof(InnerConverter<>).MakeGenericType(elementType);
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            converterType, 
            BindingFlags.Instance | BindingFlags.Public, 
            Type.DefaultBinder,
            [options],
            null
        )!;

        return converter;
    }

    private sealed class InnerConverter<TElement> : JsonConverter<ResourceRegistry<TElement>> where TElement : IResourceRegistryElement {
        private readonly JsonConverter<ResourceID> _resourceIdConverter;
        private readonly JsonConverter<TElement> _elementConverter;

        public InnerConverter(JsonSerializerOptions options) {
            _resourceIdConverter = (JsonConverter<ResourceID>)options.GetConverter(typeof(ResourceID));
            _elementConverter = (JsonConverter<TElement>)options.GetConverter(typeof(TElement));
        }
        
        public override ResourceRegistry<TElement> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException();
            }

            ResourceRegistry<TElement> registry = [];

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    return registry;
                }
                
                if (reader.TokenType != JsonTokenType.PropertyName) {
                    throw new JsonException();
                }
                
                ResourceID resourceID = _resourceIdConverter.Read(ref reader, ResourceIDType, options);

                reader.Read();

                if (_elementConverter.Read(ref reader, typeof(TElement), options) is { } element) {
                    registry[resourceID] = element;
                }
            }
            
            throw new JsonException();
        }
        
        public override void Write(Utf8JsonWriter writer, ResourceRegistry<TElement> value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            
            foreach ((var resourceId, var element) in value) {
                _resourceIdConverter.WriteAsPropertyName(writer, resourceId, options);
                _elementConverter.Write(writer, element, options);
            }
            
            writer.WriteEndObject();
        }
    }
}
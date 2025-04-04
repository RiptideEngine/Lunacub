using System.Text.Json.Serialization.Metadata;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class OptionsTypeInfoResolver : DefaultJsonTypeInfoResolver {
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type.IsAssignableTo(typeof(IImportOptions))) {
            jsonTypeInfo.PolymorphismOptions = new() {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes = {
                    new(typeof(OptionsResourceDTO.Options), "OptionsResource.Options"),
                },
            };
        }

        return jsonTypeInfo;
    }
}
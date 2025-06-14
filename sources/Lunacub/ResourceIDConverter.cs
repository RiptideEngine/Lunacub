using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

internal sealed class ResourceIDConverter : JsonConverter<ResourceID> {
    public override void Write(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, Unsafe.BitCast<ResourceID, UInt128>(value), options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        ((JsonConverter<UInt128>)options.GetConverter(typeof(UInt128))).WriteAsPropertyName(writer, Unsafe.BitCast<ResourceID, UInt128>(value), options);
    }

    public override ResourceID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return Unsafe.BitCast<UInt128, ResourceID>(JsonSerializer.Deserialize<UInt128>(ref reader, options));
    }

    public override ResourceID ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return ((JsonConverter<UInt128>)options.GetConverter(typeof(UInt128))).ReadAsPropertyName(ref reader, typeToConvert, options);
    }
}
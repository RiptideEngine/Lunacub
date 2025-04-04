using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[ExcludeFromCodeCoverage]
internal sealed class ResourceIDConverter : JsonConverter<ResourceID> {
    public override void Write(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        Span<byte> bytes = stackalloc byte[32];
        bool format = value.TryFormat(bytes, out _, ReadOnlySpan<char>.Empty, null);
        Debug.Assert(format);
        
        writer.WriteStringValue(bytes);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        Span<byte> bytes = stackalloc byte[32];
        bool format = value.TryFormat(bytes, out _, ReadOnlySpan<char>.Empty, null);
        Debug.Assert(format);
        
        writer.WritePropertyName(bytes);
    }

    public override ResourceID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return ResourceID.Parse(reader.GetString()!);
    }

    public override ResourceID ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return ResourceID.Parse(reader.GetString()!);
    }
}
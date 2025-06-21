using System.Buffers;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Serialization;

internal sealed class ResourceIDConverter : JsonConverter<ResourceID> {
    public override void Write(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        Span<byte> buffer = stackalloc byte[39];
        bool format = value.TryFormat(buffer, out int bytesWritten, ReadOnlySpan<char>.Empty, CultureInfo.InvariantCulture);
        Debug.Assert(format);
        
        writer.WriteRawValue(buffer[..bytesWritten]);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ResourceID value, JsonSerializerOptions options) {
        Span<byte> buffer = stackalloc byte[39];
        bool format = value.TryFormat(buffer, out int bytesWritten, ReadOnlySpan<char>.Empty, CultureInfo.InvariantCulture);
        Debug.Assert(format);
        
        writer.WritePropertyName(buffer[..bytesWritten]);
    }

    public override ResourceID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return ReadCore(ref reader);
    }

    public override ResourceID ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return ReadCore(ref reader);
    }

    private ResourceID ReadCore(ref Utf8JsonReader reader) {
        byte[]? rentedArray = null;

        try {
            scoped Span<byte> span;
            int length;
            
            if (reader.HasValueSequence) {
                span = rentedArray = ArrayPool<byte>.Shared.Rent(length = (int)reader.ValueSequence.Length);
                reader.ValueSequence.CopyTo(span);
            } else {
                length = reader.ValueSpan.Length;

                if (length > 256) {
                    span = rentedArray = ArrayPool<byte>.Shared.Rent(length);
                } else {
                    span = stackalloc byte[length];
                }
                
                reader.ValueSpan.CopyTo(span);
            }
            
            if (ResourceID.TryParse(span[..length], CultureInfo.InvariantCulture, out ResourceID result)) return result;

            throw new FormatException("Expected valid ResourceID.");
        } finally {
            if (rentedArray != null) {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }
}
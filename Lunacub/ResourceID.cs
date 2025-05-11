using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceIDConverter))]
[CLSCompliant(false)]
public readonly struct ResourceID : IEquatable<ResourceID>, IEquatable<UInt128>, ISpanFormattable, IUtf8SpanFormattable {
    public static ResourceID Null => default;

    public UInt128 Value { get; }
    
    public ResourceID(UInt128 value) {
        Value = value;
    }

    public ResourceID(string str) {
        this = Parse(str);
    }

    public static ResourceID Create() => Unsafe.BitCast<Guid, ResourceID>(Guid.NewGuid());
    
    public static ResourceID Parse(string str) => Parse(str.AsSpan());
    public static ResourceID Parse(ReadOnlySpan<char> str) {
        if (TryParse(str, out var rid)) return rid;

        throw new FormatException("Unrecognized ResourceID format.");
    }

    public static bool TryParse([NotNullWhen(true)] string? str, out ResourceID result) => TryParse(str.AsSpan(), out result);
    
    public static bool TryParse(ReadOnlySpan<char> str, out ResourceID result) {
        str = str.Trim();

        if (UInt128.TryParse(str, NumberStyles.HexNumber, null, out var value)) {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }

    public string ToString(string? format) => Value.ToString(format);
    public string ToString(IFormatProvider? formatProvider) => Value.ToString(formatProvider);
    public string ToString(string? format, IFormatProvider? formatProvider) {
        return Value.ToString(format, formatProvider);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out charsWritten, "X", provider);
    }

    public bool TryFormat(Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out bytesWritten, "X", provider);
    }

    public bool Equals(ResourceID other) => Value == other.Value;
    public bool Equals(UInt128 other) => Value == other;

    public override bool Equals([NotNullWhen(true)] object? other) {
        return other switch {
            ResourceID rid => Equals(rid),
            UInt128 u128 => Equals(u128),
            _ => false,
        };
    }
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(ResourceID left, ResourceID right) => left.Equals(right);
    public static bool operator !=(ResourceID left, ResourceID right) => !left.Equals(right);
    
    public static implicit operator ResourceID(UInt128 value) => new(value);
    public static implicit operator UInt128(ResourceID value) => value.Value;
    
    public override string ToString() => Value.ToString();
}
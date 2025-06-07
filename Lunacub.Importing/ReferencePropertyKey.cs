namespace Caxivitual.Lunacub.Importing;

public readonly struct ReferencePropertyKey : IEquatable<ReferencePropertyKey>, IEquatable<ulong>, IUtf8SpanFormattable, ISpanFormattable {
    public readonly ulong Value;

    public ReferencePropertyKey(ulong value) {
        Value = value;
    }

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(utf8Destination, out bytesWritten, format, provider);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    public bool Equals(ReferencePropertyKey other) => Value == other.Value;
    public bool Equals(ulong other) => Value == other;

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj switch {
            ReferencePropertyKey other => Equals(other),
            ulong other => Equals(other),
            _ => false,
        };
    }
    
    public override int GetHashCode() => Value.GetHashCode();

    public string ToString(string? format) => Value.ToString(format);
    public string ToString(IFormatProvider? formatProvider) => Value.ToString(formatProvider);
    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
    public override string ToString() => Value.ToString();
    
    public static bool operator ==(ReferencePropertyKey left, ReferencePropertyKey right) => left.Equals(right);
    public static bool operator !=(ReferencePropertyKey left, ReferencePropertyKey right) => !left.Equals(right);
    
    public static implicit operator ReferencePropertyKey(ulong value) => new(value);
    public static implicit operator ulong(ReferencePropertyKey value) => value.Value;
}
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(ResourceIDConverter))]
public readonly struct ResourceID : IEquatable<ResourceID>, IUtf8SpanFormattable {
    public static ResourceID Null => default;
    
    private readonly Guid _guid;

    public ResourceID(string str) {
        this = Parse(str);
    }

    public ResourceID(Guid guid) {
        _guid = guid;
    }

    public static ResourceID Parse(string s) => Parse(s.AsSpan());
    public static ResourceID Parse(ReadOnlySpan<char> s) => new(Guid.Parse(s));

    public static bool TryParse([NotNullWhen(true)] string? s, out ResourceID result) => TryParse(s.AsSpan(), out result);
    
    public static bool TryParse(ReadOnlySpan<char> s, out ResourceID result) {
        unsafe {
            fixed (ResourceID* ptr = &result) {
                return Guid.TryParse(s, out *(Guid*)ptr);
            }
        }
    }

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return _guid.TryFormat(utf8Destination, out bytesWritten, "N");
    }

    public bool Equals(ResourceID other) => _guid == other._guid;

    public override bool Equals([NotNullWhen(true)] object? other) => other is ResourceID rid && Equals(rid);
    public override int GetHashCode() => _guid.GetHashCode();
    
    public static bool operator ==(ResourceID left, ResourceID right) => left.Equals(right);
    public static bool operator !=(ResourceID left, ResourceID right) => !left.Equals(right);

    public override string ToString() => _guid.ToString("N");
}
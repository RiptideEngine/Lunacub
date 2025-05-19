using System.Globalization;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

/// <summary>
/// Represents an identification number of a resource.
/// </summary>
[JsonConverter(typeof(ResourceIDConverter))]
[CLSCompliant(false)]
public readonly struct ResourceID : IEquatable<ResourceID>, IEquatable<UInt128>, ISpanFormattable, IUtf8SpanFormattable {
    /// <summary>
    /// Represents a default or null value of the <see cref="ResourceID"/> type, used to signify the absence of a
    /// valid resource identifier.
    /// </summary>
    public static ResourceID Null => default;

    /// <summary>
    /// The underlying 128-bit unsigned integer value of <see cref="ResourceID"/>.
    /// </summary>
    public UInt128 Value { get; }
    
    /// <summary>
    /// Creates a new instance of <see cref="ResourceID"/> with the specified 128-bit unsigned integer value.
    /// </summary>
    /// <param name="value">The specified 128-bit unsigned integer value to create <see cref="ResourceID"/> from.</param>
    public ResourceID(UInt128 value) {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ResourceID"/> with the specified string value represents a 128-bit
    /// unsigned integer value.
    /// </summary>
    /// <param name="str">The specified string represents a 128-bit unsigned hexadecimal integer value.</param>
    /// <seealso cref="Parse(ReadOnlySpan{char})"/>
    public ResourceID(ReadOnlySpan<char> str) {
        this = Parse(str);
    }

    /// <summary>
    /// Creates a random <see cref="ResourceID"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ResourceID"/> instance with random <see cref="Value"/>.</returns>
    public static ResourceID Create() => Unsafe.BitCast<Guid, ResourceID>(Guid.NewGuid());
    
    /// <summary>
    /// Parses a string into a <see cref="ResourceID"/>.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned hexadecimal integer to parse.</param>
    /// <returns>The result of parsing <paramref name="str"/>.</returns>
    public static ResourceID Parse(string str) => Parse(str.AsSpan());
    
    /// <summary>
    /// Parses a span of characters into a <see cref="ResourceID"/>.
    /// </summary>
    /// <param name="str">A span of character represents a 128-bit unsigned hexadecimal integer to parse..</param>
    /// <returns>The result of parsing <paramref name="str"/>.</returns>
    public static ResourceID Parse(ReadOnlySpan<char> str) {
        if (TryParse(str, out var rid)) return rid;

        throw new FormatException("Unrecognized ResourceID format.");
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="ResourceID"/>.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned hexadecimal integer to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="str"/>, or the default
    ///     value of <see cref="ResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="str"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse([NotNullWhen(true)] string? str, out ResourceID result) => TryParse(str.AsSpan(), out result);
    
    /// <summary>
    /// Tries to parse a span of character into a <see cref="ResourceID"/>.
    /// </summary>
    /// <param name="str">A span of character represents a 128-bit unsigned hexadecimal integer to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="str"/>, or the default
    ///     value of <see cref="ResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="str"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> str, out ResourceID result) {
        str = str.Trim();
        
        if (UInt128.TryParse(str, NumberStyles.HexNumber, null, out var value)) {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Formats this instance into numerical representation.
    /// </summary>
    /// <param name="format">The format to use, or <see langword="null"/> to use the hexadecimal format.</param>
    /// <returns>The string presentation of this instance specified by the format.</returns>
    public string ToString(string? format) => Value.ToString(format ?? "X");
    
    /// <summary>
    /// Formats this instance into numerical representation.
    /// </summary>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of this instance specified by the format.</returns>
    public string ToString(IFormatProvider? formatProvider) => Value.ToString(formatProvider);
    
    /// <summary>
    /// Formats this instance into numerical representation.
    /// </summary>
    /// <param name="format">The format to use, or <see langword="null"/> to use the hexadecimal format.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of this instance specified by the format.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider) {
        return Value.ToString(format, formatProvider);
    }

    /// <summary>
    /// Tries to format this instance into numerical representation into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
    /// <param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination"/>.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination"/>.</param>
    /// <returns></returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out charsWritten, "X", provider);
    }

    /// <inheritdoc cref="TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider)"/>
    public bool TryFormat(Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out bytesWritten, "X", provider);
    }

    /// <summary>
    /// Determines whether the specified <see cref="ResourceID"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ResourceID"/> to compare with the current instance.</param>
    /// <returns>
    ///     <see langword="true"/> if the specified <see cref="ResourceID"/> is equal to the current instance;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(ResourceID other) => Value == other.Value;
    
    /// <summary>
    /// Determines whether the <see cref="Value"/> of the current instance is equal to the specified 128-bit unsigned
    /// integer.
    /// </summary>
    /// <param name="other">The 128-bit unsigned integer to compare with the <see cref="Value"/> of the current instance.</param>
    /// <returns>
    ///     <see langword="true"/> if the <see cref="Value"/> of the current instance is equal to the provided 128-bit
    ///     unsigned integer; otherwise, <see langword="false"/>
    /// </returns>
    public bool Equals(UInt128 other) => Value == other;

    /// <summary>
    /// Determines whether this instance and a specified object, which can be either an instance of <see cref="ResourceID"/>
    /// or a 128-bit unsigned integer, are equal.
    /// </summary>
    /// <param name="other">The object to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="other"/> is a <see cref="ResourceID"/> and is equal to the current instance,
    ///     or if <paramref name="other"/> is a 128-bit unsigned integer and is equal to the <see cref="Value"/> of the current
    ///     instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Equals(ResourceID)"/>
    /// <seealso cref="Equals(UInt128)"/>
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
    
    public static implicit operator ResourceID(uint value) => new(value);
    public static implicit operator ResourceID(ulong value) => new(value);
    public static implicit operator ResourceID(UInt128 value) => new(value);
    public static implicit operator UInt128(ResourceID value) => value.Value;

    /// <summary>
    /// Returns the string representation of this instance in hexadecimal integer format.
    /// </summary>
    /// <returns>A string that represents the current <see cref="ResourceID"/> in hexadecimal integer format.</returns>
    public override string ToString() => Value.ToString("X");
}
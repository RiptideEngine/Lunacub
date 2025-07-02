using System.Globalization;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents an identification number of a resource.
/// </summary>
public readonly struct ProceduralResourceID :
    IEquatable<ProceduralResourceID>,
    IEquatable<UInt128>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<ProceduralResourceID>,
    IUtf8SpanParsable<ProceduralResourceID> {
    /// <summary>
    /// The underlying 128-bit unsigned integer value of <see cref="ProceduralResourceID"/>.
    /// </summary>
    public UInt128 Value { get; }
    
    /// <summary>
    /// Creates a new instance of <see cref="ProceduralResourceID"/> with the specified 128-bit unsigned integer value.
    /// </summary>
    /// <param name="value">The specified 128-bit unsigned integer value to create <see cref="ProceduralResourceID"/> from.</param>
    public ProceduralResourceID(UInt128 value) {
        Value = value;
    }

    /// <summary>
    /// Creates a random <see cref="ProceduralResourceID"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ProceduralResourceID"/> instance with random <see cref="Value"/>.</returns>
    public static ProceduralResourceID Create() => Unsafe.BitCast<Guid, ProceduralResourceID>(Guid.NewGuid());

    /// <summary>
    /// Parses a string into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned integer to parse.</param>
    /// <returns>The result of parsing <paramref name="str"/>.</returns>
    public static ProceduralResourceID Parse(string str) => new(UInt128.Parse(str));

    /// <summary>
    /// Parse a string into a <see cref="ProceduralResourceID"/> with formatting information object.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned integer to parse.</param>
    /// <param name="formatProvider">An object contains formatting information about <paramref name="str"/>.</param>
    /// <returns></returns>
    public static ProceduralResourceID Parse(string str, IFormatProvider? formatProvider) {
        return new(UInt128.Parse(str, formatProvider));
    }
    
    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    public static ProceduralResourceID Parse(ReadOnlySpan<byte> span) => new(UInt128.Parse(span));

    /// <inheritdoc cref="Parse(ReadOnlySpan{char},IFormatProvider?)"/>
    public static ProceduralResourceID Parse(ReadOnlySpan<byte> span, IFormatProvider? formatProvider) {
        return new(UInt128.Parse(span, formatProvider));
    }
    
    /// <summary>
    /// Parses a span of characters into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="span">A span of character represents a 128-bit unsigned integer to parse.</param>
    /// <returns>The result of parsing <paramref name="span"/>.</returns>
    public static ProceduralResourceID Parse(ReadOnlySpan<char> span) => new(UInt128.Parse(span));

    /// <summary>
    /// Parses a span of characters into a <see cref="ProceduralResourceID"/> with formatting information object.
    /// </summary>
    /// <param name="span">A span of character represents a 128-bit unsigned integer to parse.</param>
    /// <param name="formatProvider">An object contains formatting information about <paramref name="span"/>.</param>
    /// <returns>The result of parsing <paramref name="span"/>.</returns>
    public static ProceduralResourceID Parse(ReadOnlySpan<char> span, IFormatProvider? formatProvider) {
        return new(UInt128.Parse(span, formatProvider));
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned integer to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="str"/>, or the default
    ///     value of <see cref="ProceduralResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="str"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse([NotNullWhen(true)] string? str, out ProceduralResourceID result) => TryParse(str.AsSpan(), out result);

    /// <summary>
    /// Tries to parse a string into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="str">A string represents a 128-bit unsigned integer to parse.</param>
    /// <param name="formatProvider">An object contains formatting information about <paramref name="str"/>.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="str"/>, or the default
    ///     value of <see cref="ProceduralResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="str"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse([NotNullWhen(true)] string? str, IFormatProvider? formatProvider, out ProceduralResourceID result) {
        return TryParse(str.AsSpan(), out result);
    }
    
    /// <summary>
    /// Tries to parse a span of character into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="span">A span of character represents a 128-bit unsigned integer to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="span"/>, or the default
    ///     value of <see cref="ProceduralResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="span"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> span, out ProceduralResourceID result) {
        if (UInt128.TryParse(span, NumberStyles.HexNumber, null, out var value)) {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <summary>
    /// Tries to parse a span of character into a <see cref="ProceduralResourceID"/>.
    /// </summary>
    /// <param name="span">A span of character represents a 128-bit unsigned integer to parse.</param>
    /// <param name="formatProvider">An object contains formatting information about <paramref name="span"/>.</param>
    /// <param name="result">
    ///     When this method returns, contains the result of successfully parsing <paramref name="span"/>, or the default
    ///     value of <see cref="ProceduralResourceID"/> if parsing failed.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="span"/> was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> span, IFormatProvider? formatProvider, out ProceduralResourceID result) {
        if (UInt128.TryParse(span, NumberStyles.HexNumber, null, out var value)) {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <inheritdoc cref="TryParse(ReadOnlySpan{char},out Caxivitual.Lunacub.Building.ProceduralResourceID)"/>
    public static bool TryParse(ReadOnlySpan<byte> span, out ProceduralResourceID result) {
        if (UInt128.TryParse(span, NumberStyles.HexNumber, null, out var value)) {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <inheritdoc cref="TryParse(ReadOnlySpan{char},IFormatProvider?,out Caxivitual.Lunacub.Building.ProceduralResourceID)"/>
    public static bool TryParse(ReadOnlySpan<byte> span, IFormatProvider? formatProvider, out ProceduralResourceID result) {
        if (UInt128.TryParse(span, NumberStyles.HexNumber, null, out var value)) {
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
        return Value.ToString(format ?? "X", formatProvider);
    }

    /// <summary>
    /// Tries to format this instance into numerical representation into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">
    ///     When this method returns, contains the number of characters that were written in <paramref name="destination"/>.
    /// </param>
    /// <param name="format">
    ///     A span containing the characters that represent a standard or custom format string that defines the acceptable format for
    ///     <paramref name="destination"/>.
    /// </param>
    /// <param name="provider">
    ///     An optional object that supplies culture-specific formatting information for <paramref name="destination"/>.
    /// </param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    /// <inheritdoc cref="TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider)"/>
    public bool TryFormat(Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
        return Value.TryFormat(destination, out bytesWritten, format, provider);
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProceduralResourceID"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ProceduralResourceID"/> to compare with the current instance.</param>
    /// <returns>
    ///     <see langword="true"/> if the specified <see cref="ProceduralResourceID"/> is equal to the current instance;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(ProceduralResourceID other) => Value == other.Value;
    
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
    /// Determines whether this instance and a specified object, which can be either an instance of <see cref="ProceduralResourceID"/>
    /// or a 128-bit unsigned integer, are equal.
    /// </summary>
    /// <param name="other">The object to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="other"/> is a <see cref="ProceduralResourceID"/> and is equal to the current
    ///     instance, or if <paramref name="other"/> is a 128-bit unsigned integer and is equal to the <see cref="Value"/> of the current
    ///     instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Equals(ProceduralResourceID)"/>
    /// <seealso cref="Equals(UInt128)"/>
    public override bool Equals([NotNullWhen(true)] object? other) {
        return other switch {
            ProceduralResourceID rid => Equals(rid),
            UInt128 u128 => Equals(u128),
            _ => false,
        };
    }
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(ProceduralResourceID left, ProceduralResourceID right) => left.Equals(right);
    public static bool operator !=(ProceduralResourceID left, ProceduralResourceID right) => !left.Equals(right);
    
    public static implicit operator ProceduralResourceID(uint value) => new(value);
    public static implicit operator ProceduralResourceID(ulong value) => new(value);
    public static implicit operator ProceduralResourceID(UInt128 value) => new(value);
    public static implicit operator UInt128(ProceduralResourceID value) => value.Value;

    /// <summary>
    /// Returns the string representation of this instance in hexadecimal integer format.
    /// </summary>
    /// <returns>A string that represents the current <see cref="ProceduralResourceID"/> in hexadecimal integer format.</returns>
    public override string ToString() => Value.ToString("X");
}
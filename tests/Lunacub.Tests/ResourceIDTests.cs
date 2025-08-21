// ReSharper disable EqualExpressionComparison

using System.Globalization;
using System.Reflection;

namespace Caxivitual.Lunacub.Tests;

public class ResourceIDTests {
    public static TheoryData<string> ValidParseIntegerString => [
        "0", "255", "65536", "2147483647", "18446744073709551615", "340282366920938463463374607431768211455",
        " 255", "255 ", " 255 ",
    ];
    
    public static TheoryData<string> InvalidParseIntegerString => [
        string.Empty, "FF", "Random_Text", "2 55", "25%1#AE125^r91*2514",
    ];

    public static TheoryData<string> NegativeParseIntegerString => ["-1", " -1", "-1 ", " -1 "];
    
    public static TheoryData<byte[]> ValidParseIntegerUtf8 => new(ValidParseIntegerString.Select(x => Encoding.UTF8.GetBytes(x.Data)));
    public static TheoryData<byte[]> InvalidParseIntegerUtf8 => new(InvalidParseIntegerString.Select(x => Encoding.UTF8.GetBytes(x.Data)));
    public static TheoryData<byte[]> NegativeParseIntegerUtf8 => new(NegativeParseIntegerString.Select(x => Encoding.UTF8.GetBytes(x.Data)));
    
    public static TheoryData<string> ValidParseHexadecimalString => [
        "0", "20", "FF", "FFFF", "FFFFFFFF", "FFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
        "deadBEEF", "C0FfeE", "3fe74",
    ];
    
    public static TheoryData<string> InvalidParseHexadecimalString => [
        string.Empty, "F F", "Random_Text", "2 55", "25%1#AE125^r91*2514"
    ];
    
    public static TheoryData<byte[]> ValidParseHexadecimalUtf8 => new(ValidParseHexadecimalString.Select(x => Encoding.UTF8.GetBytes(x.Data)));
    public static TheoryData<byte[]> InvalidParseHexadecimalUtf8 => new(InvalidParseHexadecimalString.Select(x => Encoding.UTF8.GetBytes(x.Data)));

    public static TheoryData<ResourceID, string> ToStringData => [
        (255, string.Empty),
        (255, "X"),
        (255, "X32"),
        (65535, string.Empty),
        (65535, "X"),
        (65535, "X32"),
        (2147483647, string.Empty),
        (2147483647, "X"),
        (2147483647, "X32"),
        (18446744073709551615, string.Empty),
        (18446744073709551615, "X"),
        (18446744073709551615, "X32"),
        (ResourceID.Parse("340282366920938463463374607431768211455"), string.Empty),
        (ResourceID.Parse("340282366920938463463374607431768211455"), "X"),
        (ResourceID.Parse("340282366920938463463374607431768211455"), "X32"),
    ];
    
    [Fact]
    public void Conversion_FromUInt32_ShouldBeCorrect() {
        ResourceID id = 25U;
        id.Value.Should().Be(25);
    }

    [Fact]
    public void Conversion_FromUInt64_ShouldBeCorrect() {
        ResourceID id = 25UL;
        id.Value.Should().Be(25);
    }
    
    [Fact]
    public void Conversion_FromUInt128_ShouldBeCorrect() {
        ResourceID id = (UInt128)25;
        id.Value.Should().Be(25);
    }

    [Fact]
    public void Converstion_ToUInt128_ShouldBeCorrect() {
        ((UInt128)ResourceID.Parse("255")).Should().Be(255);
    }

    [Theory]
    [MemberData(nameof(ValidParseIntegerString))]
    public void ParseIntegerFromString_Valid_ShouldParsesCorrectly(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input));
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input));
        new Func<ResourceID>(() => ResourceID.Parse(input.AsSpan(), null)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input));
    }
    
    [Theory]
    [MemberData(nameof(InvalidParseIntegerString))]
    public void ParseIntegerFromString_Invalid_ShouldThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<FormatException>();
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().Throw<FormatException>();
        new Func<ResourceID>(() => ResourceID.Parse(input.AsSpan(), null)).Should().Throw<FormatException>();
    }

    [Theory]
    [MemberData(nameof(NegativeParseIntegerString))]
    public void ParseIntegerFromString_Negative_ShouldThrowsOverflowException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<OverflowException>();
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().Throw<OverflowException>();
        new Func<ResourceID>(() => ResourceID.Parse(input.AsSpan(), null)).Should().Throw<OverflowException>();
    }

    [Theory]
    [MemberData(nameof(ValidParseIntegerUtf8))]
    public void ParseIntegerFromUtf8_Valid_ShouldParsesCorrectly(byte[] input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input));
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input));
    }

    [Theory]
    [MemberData(nameof(InvalidParseIntegerUtf8))]
    public void ParseIntegerFromUtf8_Invalid_ShouldThrowsFormatException(byte[] input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<FormatException>();
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().Throw<FormatException>();
    }

    [Theory]
    [MemberData(nameof(NegativeParseIntegerUtf8))]
    public void ParseIntegerFromUtf8_Negative_ShouldThrowsOverflowException(byte[] input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<OverflowException>();
        new Func<ResourceID>(() => ResourceID.Parse(input, null)).Should().Throw<OverflowException>();
    }
    
    [Theory]
    [MemberData(nameof(ValidParseHexadecimalString))]
    public void ParseHexadecimalFromString_Valid_ShouldParsesCorrectly(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input, NumberStyles.HexNumber));
    }
    
    [Theory]
    [MemberData(nameof(InvalidParseHexadecimalString))]
    public void ParseHexadecimalFromString_Invalid_ShouldThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().Throw<FormatException>();
    }

    [Theory]
    [MemberData(nameof(ValidParseHexadecimalUtf8))]
    public void ParseHexadecimalFromUtf8_Valid_ShouldParsesCorrectly(byte[] input) {
        new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().NotThrow().Which.Value.Should().Be(UInt128.Parse(input, NumberStyles.HexNumber));
    }

    [Theory]
    [MemberData(nameof(InvalidParseHexadecimalUtf8))]
    public void ParseHexadecimalFromUtf8_Invalid_ShouldThrowsFormatException(byte[] input) {
        new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().Throw<FormatException>();
    }
    
    [Theory]
    [MemberData(nameof(ValidParseIntegerString))]
    public void TryParseIntegerFromString_Valid_ShouldReturnsTrueAndCorrectValue(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID output);
            return (parse, output.Value);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input)));
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID output);
            return (parse, output);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input)));
    }
    
    [Theory]
    [MemberData(nameof(InvalidParseIntegerString))]
    public void TryParseIntegerFromString_Invalid_ShouldReturnsFalseAndDefaultValue(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID output);
            return (parse, output.Value);
        }).Should().NotThrow().Which.Should().Be((false, default));
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID output);
            return (parse, output);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }
    
    [Theory]
    [MemberData(nameof(NegativeParseIntegerString))]
    public void TryParseIntegerFromString_Negative_ShouldReturnsFalseAndDefaultValue(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID output);
            return (parse, output.Value);
        }).Should().NotThrow().Which.Should().Be((false, default));
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID output);
            return (parse, output);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }
    
    [Theory]
    [MemberData(nameof(ValidParseIntegerUtf8))]
    public void TryParseIntegerFromUtf8_Valid_ShouldReturnsTrueAndCorrectValue(byte[] input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input)));
        
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input)));
    }

    [Theory]
    [MemberData(nameof(InvalidParseIntegerUtf8))]
    public void TryParseIntegerFromUtf8_Invalid_ShouldReturnsFalseAndDefaultValue(byte[] input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
        
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }

    [Theory]
    [MemberData(nameof(NegativeParseIntegerUtf8))]
    public void TryParseIntegerFromUtf8_Negative_ShouldReturnsFalseAndDefaultValue(byte[] input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
        
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.Integer, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }
    
    [Theory]
    [MemberData(nameof(ValidParseHexadecimalString))]
    public void TryParseHexadecimalFromString_Valid_ShouldReturnsTrueAndCorrectValue(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.HexNumber, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input, NumberStyles.HexNumber)));
    }
    
    [Theory]
    [MemberData(nameof(InvalidParseHexadecimalString))]
    public void TryParseHexadecimalFromString_Invalid_ShouldReturnsFalseAndDefaultValue(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.HexNumber, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }

    [Theory]
    [MemberData(nameof(ValidParseHexadecimalUtf8))]
    public void TryParseHexadecimalFromUtf8_Valid_ShouldReturnsTrueAndCorrectValue(byte[] input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.HexNumber, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input, NumberStyles.HexNumber)));
    }
    
    [Theory]
    [MemberData(nameof(InvalidParseHexadecimalUtf8))]
    public void TryParseHexadecimalFromUtf8_Invalid_ShouldReturnsFalseAndDefaultValue(byte[] input) {
        new Func<(bool, ResourceID)>(() => {
            bool parse = ResourceID.TryParse(input, NumberStyles.HexNumber, null, out ResourceID result);
            return (parse, result);
        }).Should().NotThrow().Which.Should().Be((false, default));
    }

    [Theory]
    [MemberData(nameof(ToStringData))]
    public void ToString_ShouldReturnsCorrectly(ResourceID id, string format) {
        id.ToString(format).Should().Be(id.Value.ToString(format));
    }
    
    [Fact]
    public void Equality_ShouldReturnsCorrectly() {
        ((ResourceID)255).Should().Be((ResourceID)255);
        ((ResourceID)255 == (ResourceID)255).Should().BeTrue();
        ((ResourceID)255 != (ResourceID)255).Should().BeFalse();
        
        ((ResourceID)255).Should().Be((UInt128)255);
        ((ResourceID)255 == (UInt128)255).Should().BeTrue();
        ((ResourceID)255 != (UInt128)255).Should().BeFalse();
    }
    
    [Fact]
    public void Inequality_ShouldReturnsCorrectly() {
        ResourceID.Parse("255").Should().NotBe((UInt128)256U);
        
        ((ResourceID)255 != (ResourceID)257).Should().BeTrue();
        ((ResourceID)255 != (UInt128)255).Should().BeFalse();
    }
}
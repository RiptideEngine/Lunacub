// ReSharper disable EqualExpressionComparison

using System.Globalization;

namespace Caxivitual.Lunacub.Tests;

public class ResourceIDTests {
    [Theory]
    [InlineData("0")]
    [InlineData("1212851287")]
    [InlineData("1982568172356712421947")]
    public void Parse_DecimalInteger_ParseCorrectly(string input) {
        UInt128 idAsInt = Unsafe.BitCast<ResourceID, UInt128>(new Func<ResourceID>(() => ResourceID.Parse(input)).Should().NotThrow().Which);
        idAsInt.Should().Be(UInt128.Parse(input));
    }
    
    [Theory]
    [InlineData("0")]
    [InlineData("526BE3718")]
    [InlineData("40e1e2f1a3d2403ca3429ab0a5d189b0")]
    public void Parse_HexadecimalInteger_ParseCorrectly(string input) {
        UInt128 idAsInt = Unsafe.BitCast<ResourceID, UInt128>(new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().NotThrow().Which);
        idAsInt.Should().Be(UInt128.Parse(input, NumberStyles.HexNumber));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("19624861724EACFFF")]
    public void Parse_InvalidDecimal_ThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<FormatException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("19#$28%6!@4E!!AFCCE[CX872]POQ$!*(@&$^*&")]
    public void Parse_InvalidHexadecimal_ThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input, NumberStyles.HexNumber)).Should().Throw<FormatException>();
    }

    [Fact]
    public void EqualityChecks_ReturnsCorrectTruthy() {
        ResourceID.Parse("255").Equals(ResourceID.Parse("255")).Should().BeTrue();
        (ResourceID.Parse("100") == ResourceID.Parse("100")).Should().BeTrue();
        (ResourceID.Parse("325") != ResourceID.Parse("655")).Should().BeTrue();
    }
}
// ReSharper disable EqualExpressionComparison

using System.Globalization;

namespace Caxivitual.Lunacub.Tests;

public class ResourceIDTests {
    [Theory]
    [InlineData("0")]
    [InlineData("526BE3718")]
    [InlineData("40e1e2f1a3d2403ca3429ab0a5d189b0")]
    public void Parse_HexadecimalInteger_ParseCorrectly(string input) {
        UInt128 idAsInt = Unsafe.BitCast<ResourceID, UInt128>(new Func<ResourceID>(() => ResourceID.Parse(input)).Should().NotThrow().Which);
        idAsInt.Should().Be(UInt128.Parse(input, NumberStyles.HexNumber));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("XXXXX")]
    [InlineData("0521a625c46a5c9fb76890ca92c7d4ed4c72064230615589bc680e3718610af4")]
    public void Parse_InvalidFormat_ThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<FormatException>();
    }
    
    [Theory]
    [InlineData("0")]
    [InlineData("526BE3718")]
    [InlineData("40e1e2f1a3d2403ca3429ab0a5d189b0")]
    public void TryParse_CorrectFormat_ReturnsTrueAndOutputCorrectValue(string input) {
        new Func<(bool, UInt128)>(() => {
            bool result = ResourceID.TryParse(input, out var output);
            return (result, Unsafe.BitCast<ResourceID, UInt128>(output));
        }).Should().NotThrow().Which.Should().Be((true, UInt128.Parse(input, NumberStyles.HexNumber)));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("Invalid Value")]
    [InlineData("0521a625c46a5c9fb76890ca92c7d4ed4c72064230615589bc680e3718610af4")]
    public void TryParse_InvalidFormat_ReturnsFalseAndOutputDefault(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool result = ResourceID.TryParse(input, out var output);
            return (result, output);
        }).Should().NotThrow().Which.Should().Be((false, ResourceID.Null));
    }

    [Fact]
    public void EqualityChecks_ReturnsCorrectTruthy() {
        ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed").Equals(ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed") == ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("54f00be059a65592a9cee17551ef4943") != ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ee")).Should().BeTrue();
    }
}
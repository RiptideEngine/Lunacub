// ReSharper disable EqualExpressionComparison

namespace Caxivitual.Lunacub.Tests;

public class ResourceIDTests {
    [Fact]
    public void Parse_CorrectFormat_ShouldBeCorrect() {
        new Func<ResourceID>(() => ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().NotThrow().Which
            .Should().Be(ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed"));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("0325ac32")]
    [InlineData("0521a625c46a5c9fb76890ca92c7d4ed4c72064230615589bc680e3718610af4")]
    public void Parse_InvalidFormat_ThrowsFormatException(string input) {
        new Func<ResourceID>(() => ResourceID.Parse(input)).Should().Throw<FormatException>();
    }
    
    [Fact]
    public void TryParse_CorrectFormat_ReturnsTrueAndOutputCorrectValue() {
        new Func<(bool, ResourceID)>(() => {
            bool result = ResourceID.TryParse("0521a625c46a5c9fb76890ca92c7d4ed", out var output);
            return (result, output);
        }).Should().NotThrow().Which.Should().Be((true, ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("0325ac32")]
    [InlineData("0521a625c46a5c9fb76890ca92c7d4ed4c72064230615589bc680e3718610af4")]
    public void TryParse_InvalidFormat_ReturnsFalseAndOutputDefault(string input) {
        new Func<(bool, ResourceID)>(() => {
            bool result = ResourceID.TryParse(input, out var output);
            return (result, output);
        }).Should().NotThrow().Which.Should().Be((false, ResourceID.Null));
    }

    [Fact]
    public void TryFormatSpan_InsufficientBuffer_ShouldNotThrow() {
        new Func<bool>(() => ResourceID.Null.TryFormat([], out _, ReadOnlySpan<char>.Empty, null)).Should().NotThrow().Which.Should().BeFalse();
    }

    [Fact]
    public void EqualityChecks_ReturnsCorrectTruthy() {
        ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed").Equals(ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed") == ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("54f00be059a65592a9cee17551ef4943") != ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ee")).Should().BeTrue();
    }
}
// ReSharper disable EqualExpressionComparison

namespace Caxivitual.Lunacub.Tests;

public class ResourceIDTests {
    [Fact]
    public void Parse_ShouldBeCorrect() {
        new Func<ResourceID>(() => ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().NotThrow().Which
            .Should().Be(ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed"));
    }
    
    [Fact]
    public void Parse_FailureShouldThrow() {
        new Func<ResourceID>(() => ResourceID.Parse(string.Empty)).Should().Throw<FormatException>();

    }
    
    [Fact]
    public void TryParse_ShouldBeCorrect() {
        new Func<bool>(() => ResourceID.TryParse("0521a625c46a5c9fb76890ca92c7d4ed", out _)).Should().NotThrow().Which.Should().BeTrue();
    }
    
    [Fact]
    public void TryParse_FailureShouldNotThrowAndShouldBeCorrect() {
        new Func<bool>(() => ResourceID.TryParse(string.Empty, out _)).Should().NotThrow().Which.Should().BeFalse();
    }

    [Fact]
    public void EqualityChecks_ShouldBeCorrect() {
        ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed").Equals(ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed") == ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed")).Should().BeTrue();
        (ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ed") != ResourceID.Parse("0521a625c46a5c9fb76890ca92c7d4ee")).Should().BeTrue();
    }
}
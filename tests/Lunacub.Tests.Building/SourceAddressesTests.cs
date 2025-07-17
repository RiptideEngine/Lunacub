using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Tests.Building;

public class SourceAddressesTests {
    [Fact]
    public void Equality_DifferentPrimary_ShouldReturnsFalse() {
        SourceAddresses a = new("A");
        SourceAddresses b = new("B");

        a.Should().NotBe(b);
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentSecondarySequentially_ShouldReturnsFalse() {
        SourceAddresses a = new("A", FrozenDictionary<string, string>.Empty);
        SourceAddresses b = new("A", new Dictionary<string, string> {
            ["B"] = "ValueB",
            ["C"] = "ValueC",
        });

        a.Should().NotBe(b);
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_SequentiallyEqualSecondary_ShouldReturnsTrue() {
        SourceAddresses a = new("A", new Dictionary<string, string> {
            ["B"] = "ValueB",
            ["C"] = "ValueC",
        });
        SourceAddresses b = new("A", new Dictionary<string, string> {
            ["B"] = "ValueB",
            ["C"] = "ValueC",
        });

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }
    
    [Fact]
    public void Equality_ReferenceEqualSecondary_ShouldReturnsTrue() {
        var dict = new Dictionary<string, string> {
            ["B"] = "ValueB",
            ["C"] = "ValueC",
        };
        
        SourceAddresses a = new("A", dict);
        SourceAddresses b = new("A", dict);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }
    
    [Fact]
    public void Equality_DifferentSecondaryLength_ShouldReturnsFalse() {
        SourceAddresses a = new("A", new Dictionary<string, string> {
            ["B"] = "ValueB",
        });
        SourceAddresses b = new("A", new Dictionary<string, string> {
            ["B"] = "ValueB",
            ["C"] = "ValueC",
        });

        a.Should().NotBe(b);
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithNull_ShouldReturnsFalse() {
        SourceAddresses a = new("A");

        a.Should().NotBe(null);
    }

    [Fact]
    public void Equality_WithDifferentObject_ShouldReturnsFalse() {
        SourceAddresses a = new("A");
        a.Should().NotBe(25.0);
    }
}
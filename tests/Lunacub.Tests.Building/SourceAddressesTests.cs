using System.Collections.Frozen;
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class SourceAddressesTests {
    [Fact]
    public void Equality_DifferentPrimary_ReturnsFalse() {
        SourceAddresses a = new("A");
        SourceAddresses b = new("B");

        a.Should().NotBe(b);
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentSecondarySequentially_ReturnsFalse() {
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
    public void Equality_SequentiallyEqualSecondary_ReturnsTrue() {
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
    public void Equality_ReferenceEqualSecondary_ReturnsTrue() {
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
    public void Equality_DifferentSecondaryLength_ReturnsFalse() {
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
    public void Equality_WithNull_ReturnsFalse() {
        SourceAddresses a = new("A");

        a.Should().NotBe(null);
    }

    [Fact]
    public void Equality_WithDifferentObject_ReturnsFalse() {
        SourceAddresses a = new("A");
        a.Should().NotBe(25.0);
    }

    [Fact]
    public void JsonSerialization_ReturnsCorrectJson() {
        SourceAddresses address = new("Primary", new Dictionary<string, string> {
            ["Secondary1"] = "Secondary1",
            ["Secondary2"] = "Secondary2",
        });

        JsonSerializer.Serialize(address).Should().Be("""{"Primary":"Primary","Secondaries":{"Secondary1":"Secondary1","Secondary2":"Secondary2"}}""");
    }

    [Fact]
    public void JsonDeserialization_ReturnsCorrectObject() {
        SourceAddresses address = new("Primary", new Dictionary<string, string> {
            ["Secondary1"] = "Secondary1",
            ["Secondary2"] = "Secondary2",
        });
        const string json = """{"Primary":"Primary","Secondaries":{"Secondary1":"Secondary1","Secondary2":"Secondary2"}}""";
        
        JsonSerializer.Deserialize<SourceAddresses>(json).Should().BeEquivalentTo(address);
    }
}
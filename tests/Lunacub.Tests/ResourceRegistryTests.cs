namespace Caxivitual.Lunacub.Tests;

public class ResourceRegistryTests {
    [Fact]
    public void ElementEquality_Name_ShouldCompareByValue() {
        ResourceRegistry.Element a = new("A", []);
        ResourceRegistry.Element b = new("B", []);
        ResourceRegistry.Element c = new("A", []);

        a.Should().NotBe(b);
        a.Should().Be(c);
    }
    
    [Fact]
    public void ElementEquality_Tags_ShouldCompareSequentially() {
        ResourceRegistry.Element a = new(string.Empty, []);
        ResourceRegistry.Element b = new(string.Empty, ["Tag1"]);
        ResourceRegistry.Element c = new(string.Empty, ["Tag2"]);
        ResourceRegistry.Element d = new(string.Empty, []);

        a.Should().NotBe(b);
        a.Should().NotBe(c);
        b.Should().NotBe(c);
        a.Should().Be(d);
        b.Should().NotBe(c);
        b.Should().NotBe(d);
    }
    
    [Fact]
    public void ElementOfTEquality_Name_ShouldCompareByValue() {
        ResourceRegistry.Element<int> a = new("A", [], 0);
        ResourceRegistry.Element<int> b = new("B", [], 0);
        ResourceRegistry.Element<int> c = new("A", [], 1);
        ResourceRegistry.Element<int> d = new("B", [], 0);

        a.Should().NotBe(b);
        a.Should().NotBe(c);
        b.Should().NotBe(c);
        b.Should().Be(d);
        c.Should().NotBe(d);
    }
    
    [Fact]
    public void ElementOfTEquality_Tags_ShouldCompareSequentially() {
        ResourceRegistry.Element<int> a = new(string.Empty, [], 0);
        ResourceRegistry.Element<int> b = new(string.Empty, ["Tag1"], 0);
        ResourceRegistry.Element<int> c = new(string.Empty, ["Tag2"], 1);
        ResourceRegistry.Element<int> d = new(string.Empty, [], 0);

        a.Should().NotBe(b);
        a.Should().NotBe(c);
        b.Should().NotBe(c);
        a.Should().Be(d);
        b.Should().NotBe(c);
        b.Should().NotBe(d);
    }

    [Fact]
    public void JsonSerialization_NonGenericElement_ReturnsCorrectJson() {
        ResourceRegistry.Element element = new("Some Resource", ["A", "B", "C"]);

        JsonSerializer.Serialize(element).Should().Be("""{"Name":"Some Resource","Tags":["A","B","C"]}""");
    }
    
    [Fact]
    public void JsonDeserialization_NonGenericElement_ReturnsCorrectObject() {
        const string json = """{"Name":"Some Resource","Tags":["A","B","C"]}""";

        JsonSerializer.Deserialize<ResourceRegistry.Element>(json).Should().Be(new ResourceRegistry.Element {
            Name = "Some Resource",
            Tags = ["A", "B", "C"],
        });
    }
    
    [Fact]
    public void JsonSerialization_GenericElement_ReturnsCorrectJson() {
        ResourceRegistry.Element<int> element = new("Some Resource", ["A", "B", "C"], 25);

        JsonSerializer.Serialize(element).Should().Be("""{"Name":"Some Resource","Tags":["A","B","C"],"Option":25}""");
    }
    
    [Fact]
    public void JsonDeserialization_GenericElement_ReturnsCorrectObject() {
        const string json = """{"Name":"Some Resource","Tags":["A","B","C"],"Option":25}""";

        JsonSerializer.Deserialize<ResourceRegistry.Element<int>>(json).Should().Be(new ResourceRegistry.Element<int> {
            Name = "Some Resource",
            Tags = ["A", "B", "C"],
            Option = 25,
        });
    }
}
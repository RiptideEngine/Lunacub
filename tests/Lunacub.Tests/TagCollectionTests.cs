using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Tests;

public class TagCollectionTests {
    [Fact]
    public void GetIndexer_ReturnsCorrectElement() {
        TagCollection tags = ["A", "B", "C", "D"];
        tags[2].Should().Be("C");
    }

    [Fact]
    public void Create_1Element_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create("A");

        tags.Should().Equal("A");
    }
    
    [Fact]
    public void Create_2Element_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create("A", "B");

        tags.Should().Equal("A", "B");
    }
    
    [Fact]
    public void Create_3Element_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create("A", "B", "C");

        tags.Should().Equal("A", "B", "C");
    }
    
    [Fact]
    public void Create_4Element_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create("A", "B", "C", "D");

        tags.Should().Equal("A", "B", "C", "D");
    }
    
    [Fact]
    public void Create_FromReadOnlySpan_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create(new[] {
            "A", "B", "C", "D", "E", "F",
        }.AsSpan());

        tags.Should().Equal("A", "B", "C", "D", "E", "F");
    }

    [Fact]
    public void Create_FromImmutableArray_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create(ImmutableArray.Create("A", "B", "C", "D", "E", "F"));

        tags.Should().Equal("A", "B", "C", "D", "E", "F");
    }
    
    [Fact]
    public void Create_FromArray_ReturnsCorrectSequence() {
        TagCollection tags = TagCollection.Create(new[] {"A", "B", "C", "D", "E", "F"});

        tags.Should().Equal("A", "B", "C", "D", "E", "F");
    }

    [Fact]
    public void JsonSerialization_ReturnsCorectJson() {
        TagCollection tags = TagCollection.Create("A", "B", "C", "D");

        JsonSerializer.Serialize(tags).Should().Be("""["A","B","C","D"]""");
    }
    
    [Fact]
    public void JsonDeserialization_ReturnsCorrectObject() {
        const string json = """["A","B","C","D"]""";

        JsonSerializer.Deserialize<TagCollection>(json).Should().Equal("A", "B", "C", "D");
    }
}
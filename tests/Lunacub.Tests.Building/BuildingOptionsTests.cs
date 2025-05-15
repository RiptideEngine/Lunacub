using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Caxivitual.Lunacub.Tests.Building;

public class BuildingOptionsTests {
    [Fact]
    public void Constructor_ImporterAndProcessorNames_InitializeCorrectly() {
        new BuildingOptions("A", "B").Should().BeEquivalentTo(new {
            ImporterName = "A",
            ProcessorName = "B",
            Tags = Array.Empty<string>(),
            Options = (IImportOptions)null!,
        });
    }
    
    [Fact]
    public void Constructor_NullTags_InitializeAsEmptyCollection() {
        new BuildingOptions("A", "B", null, new TestOptions(0)).Tags.Should().BeEmpty();
    }
    
    [Fact]
    public void Constructor_AllParameters_InitializeCorrectly() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(0)).Should().BeEquivalentTo(new {
            ImporterName = "A",
            ProcessorName = "B",
            Tags = (string[])["C", "D"],
            Options = new TestOptions(0),
        });
    }

    [Fact]
    public void Equals_SameProperties_ReturnsTrue() {
        new BuildingOptions("A", "B", null, new TestOptions(10)).Equals(new("A", "B", null, new TestOptions(10))).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentImporterName_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(20)).Equals(new("X", "B", ["C", "D"], new TestOptions(20))).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_DifferentProcessorName_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(20)).Equals(new("A", "X", ["C", "D"], new TestOptions(20))).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_DifferenceTagsNull_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(20)).Equals(new("A", "B", null, new TestOptions(20))).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_DifferenceTags_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(20)).Equals(new("A", "B", ["C", "D", "E"], new TestOptions(20))).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_DifferenceOptionsNull_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(20)).Equals(new("A", "B", ["C", "D"], null)).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_DifferenceOptionsValue_ReturnsFalse() {
        new BuildingOptions("A", "B", ["C", "D"], new TestOptions(0)).Equals(new("A", "B", ["C", "D"], new TestOptions(1))).Should().BeFalse();
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerOptions.Default) {
        TypeInfoResolver = new ImportOptionsTypeResolver(),
    };

    [Fact]
    public void JsonSerialize_Serialize_ExpectedJson() {
        JsonSerializer.Serialize(new BuildingOptions {
            ImporterName = "A",
            ProcessorName = "B",
            Tags = ["C", "D"],
            Options = new TestOptions(10),
        }, SerializerOptions).Should().Be("""{"ImporterName":"A","ProcessorName":"B","Tags":["C","D"],"Options":{"$type":"TestOptions","Value":10}}""");
    }
    
    [Fact]
    public void JsonSerialize_Deserialize_ExpectedObject() {
        JsonSerializer.Deserialize<BuildingOptions>("""{"ImporterName":"A","ProcessorName":"B","Tags":["C","D","E","F"],"Options":{"$type":"TestOptions","Value":255}}""", SerializerOptions).Should().BeEquivalentTo(new {
            ImporterName = "A",
            ProcessorName = "B",
            Tags = (string[])["C", "D", "E", "F"],
            Options = new TestOptions(255),
        });
    }

    private record TestOptions(int Value) : IImportOptions;

    private sealed class ImportOptionsTypeResolver : DefaultJsonTypeInfoResolver {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
            var typeInfo = base.GetTypeInfo(type, options);
            
            if (typeInfo.Type == typeof(IImportOptions)) {
                typeInfo.PolymorphismOptions = new() {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes = {
                        new(typeof(TestOptions), "TestOptions"),
                    },
                };
            }

            return typeInfo;
        }
    }
}
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class BuildingOptionsTests {
    [Fact]
    public void JsonSerialization_NullOptions_ReturnsCorrectJson() {
        BuildingResource resource = new(new("P", new Dictionary<string, string> {
            ["Secondary1"] = "S1",
            ["Secondary2"] = "S2",
        }), new("SomeImporter", "SomeProcessor", null));

        JsonSerializer.Serialize(resource).Should().Be("""{"Addresses":{"Primary":"P","Secondaries":{"Secondary1":"S1","Secondary2":"S2"}},"Options":{"ImporterName":"SomeImporter","ProcessorName":"SomeProcessor"}}""");
    }
    
    [Fact]
    public void JsonDeserialization_NullOptions_ReturnsCorrectObject() {
        const string json = """{"Addresses":{"Primary":"P","Secondaries":{"Secondary1":"S1","Secondary2":"S2"}},"Options":{"ImporterName":"SomeImporter","ProcessorName":"SomeProcessor"}}""";

        BuildingResource resource = new Func<BuildingResource>(() => JsonSerializer.Deserialize<BuildingResource>(json)).Should().NotThrow().Which;

        resource.Should().BeEquivalentTo(new {
            Addresses = new SourceAddresses("P", new Dictionary<string, string> {
                ["Secondary1"] = "S1",
                ["Secondary2"] = "S2",
            }),
            Options = new BuildingOptions("SomeImporter", "SomeProcessor", null),
        });
    }
}
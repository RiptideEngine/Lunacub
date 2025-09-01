using Caxivitual.Lunacub.Building.Incremental;
using FluentAssertions.Extensions;
using System.Text;
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class SourcesInfoTests {
    [Fact]
    public void JsonSerializing_ShouldSerializeCorrectly() {
        DateTime time = DateTime.Today;
        
        SourceInfo primary = new("Primary", time);
        SourceInfo secondary1 = new("Secondary1", time - 1.Days());
        SourceInfo secondary2 = new("Secondary2", time - 2.Days());
        SourceInfo secondary3 = new("Secondary3", time - 3.Days());

        string serialized = JsonSerializer.Serialize(new SourcesInfo(primary, new Dictionary<string, SourceInfo> {
            ["Secondary1"] = secondary1,
            ["Secondary2"] = secondary2,
            ["Secondary3"] = secondary3,
        }));

        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("Primary");
                    JsonSerializer.Serialize(writer, primary);
                    
                    writer.WriteStartObject("Secondaries");
                    {
                        writer.WritePropertyName("Secondary1");
                        JsonSerializer.Serialize(writer, secondary1);
                        
                        writer.WritePropertyName("Secondary2");
                        JsonSerializer.Serialize(writer, secondary2);
                        
                        writer.WritePropertyName("Secondary3");
                        JsonSerializer.Serialize(writer, secondary3);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            
            serialized.Should().Be(Encoding.UTF8.GetString(stream.ToArray()));
        }
    }
    
    [Fact]
    public void JsonDeserializing_ShouldSerializeCorrectly() {
        DateTime time = DateTime.Today;
        
        SourceInfo primary = new("Primary", time);
        SourceInfo secondary1 = new("Secondary1", time - 1.Days());
        SourceInfo secondary2 = new("Secondary2", time - 2.Days());
        SourceInfo secondary3 = new("Secondary3", time - 3.Days());

        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("Primary");
                    JsonSerializer.Serialize(writer, primary);
                    
                    writer.WriteStartObject("Secondaries");
                    {
                        writer.WritePropertyName("Secondary1");
                        JsonSerializer.Serialize(writer, secondary1);
                        
                        writer.WritePropertyName("Secondary2");
                        JsonSerializer.Serialize(writer, secondary2);
                        
                        writer.WritePropertyName("Secondary3");
                        JsonSerializer.Serialize(writer, secondary3);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            
            SourcesInfo deserialized = JsonSerializer.Deserialize<SourcesInfo>(Encoding.UTF8.GetString(stream.ToArray()));

            deserialized.Primary.Should().Be(primary);
            deserialized.Secondaries.Should().BeEquivalentTo(new Dictionary<string, SourceInfo> {
                ["Secondary1"] = secondary1,
                ["Secondary2"] = secondary2,
                ["Secondary3"] = secondary3,
            });
        }
    }
}
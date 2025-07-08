using FluentAssertions.Extensions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class SourceLastWriteTimesTests {
    [Fact]
    public void JsonSerializing_ShouldSerializeCorrectly() {
        DateTime primary = DateTime.Now;
        DateTime secondary1 = primary - 1.Days();
        DateTime secondary2 = primary - 2.Days();
        DateTime secondary3 = primary - 5.Days();

        string serialized = JsonSerializer.Serialize(new SourceLastWriteTimes(primary, new Dictionary<string, DateTime> {
            ["Secondary1"] = secondary1,
            ["Secondary2"] = secondary2,
            ["Secondary3"] = secondary3,
        }));

        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WriteString("Primary", primary);
                    
                    writer.WriteStartObject("Secondaries");
                    {
                        writer.WriteString("Secondary1", secondary1);
                        writer.WriteString("Secondary2", secondary2);
                        writer.WriteString("Secondary3", secondary3);
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
        DateTime primary = DateTime.Now;
        DateTime secondary1 = primary - 1.Days();
        DateTime secondary2 = primary - 2.Days();
        DateTime secondary3 = primary - 5.Days();

        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WriteString("Primary", primary);
                    
                    writer.WriteStartObject("Secondaries");
                    {
                        writer.WriteString("Secondary1", secondary1);
                        writer.WriteString("Secondary2", secondary2);
                        writer.WriteString("Secondary3", secondary3);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            
            string json = Encoding.UTF8.GetString(stream.ToArray());
            
            SourceLastWriteTimes deserialized = JsonSerializer.Deserialize<SourceLastWriteTimes>(json);

            deserialized.Primary.Should().Be(primary);
            deserialized.Secondaries.Should().ContainKey("Secondary1").WhoseValue.Should().Be(secondary1);
            deserialized.Secondaries.Should().ContainKey("Secondary2").WhoseValue.Should().Be(secondary2);
            deserialized.Secondaries.Should().ContainKey("Secondary3").WhoseValue.Should().Be(secondary3);
        }
    }
}
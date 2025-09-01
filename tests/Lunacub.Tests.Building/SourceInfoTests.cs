using Caxivitual.Lunacub.Building.Incremental;
using FluentAssertions.Extensions;
using System.Text;
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class SourceInfoTests {
    [Fact]
    public void JsonSerializing_ShouldSerializeCorrectly() {
        SourceInfo info = new("SourceA", DateTime.Now - 1.Days());

        string serialized = JsonSerializer.Serialize(info);

        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WriteString(nameof(SourceInfo.Address), info.Address);
                    writer.WriteString(nameof(SourceInfo.LastWriteTime), info.LastWriteTime);
                }
                writer.WriteEndObject();
            }
            
            serialized.Should().Be(Encoding.UTF8.GetString(stream.ToArray()));
        }
    }
    
    [Fact]
    public void JsonDeserializing_ShouldSerializeCorrectly() {
        DateTime time = DateTime.Now - 1.Days();
        
        using (MemoryStream stream = new MemoryStream()) {
            using (Utf8JsonWriter writer = new(stream)) {
                writer.WriteStartObject();
                {
                    writer.WriteString(nameof(SourceInfo.Address), "SourceA");
                    writer.WriteString(nameof(SourceInfo.LastWriteTime), time);
                }
                writer.WriteEndObject();
            }
            
            SourceInfo deserialized = JsonSerializer.Deserialize<SourceInfo>(Encoding.UTF8.GetString(stream.ToArray()));
            
            deserialized.Should().Be(new SourceInfo("SourceA", time));
        }
    }
}
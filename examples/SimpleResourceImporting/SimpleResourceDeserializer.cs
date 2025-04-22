using System.Numerics;
using System.Text;

namespace Caxivitual.Lunacub.Examples.SimpleResourceImporting;

public sealed class SimpleResourceDeserializer : Deserializer<SimpleResource> {
    protected override SimpleResource Deserialize(Stream dataStream, Stream optionStream, DeserializationContext context) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int integer = reader.ReadInt32();
        float single = reader.ReadSingle();
        Vector2 vector = reader.ReadVector2();
        
        return new SimpleResource(integer, single, vector);
    }
}
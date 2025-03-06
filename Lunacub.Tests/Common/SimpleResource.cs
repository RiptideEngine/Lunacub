namespace Caxivitual.Lunacub.Tests.Common;

public sealed class SimpleResource {
    public int Value { get; set; }
}

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value { get; set; }
}

public sealed class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(Stream stream) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(stream)!;
    }
}

public sealed class SimpleResourceSerializer : Serializer<SimpleResourceDTO> {
    public override string DeserializerName => nameof(SimpleResourceDeserializer);

    protected override void Serialize(SimpleResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        
        writer.Write(input.Value);
    }
}

public sealed class SimpleResourceDeserializer : Deserializer<SimpleResource> {
    protected override SimpleResource Deserialize(Stream stream, DeserializationContext context) {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        
        return new() {
            Value = reader.ReadInt32(),
        };
    }
}
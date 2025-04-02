namespace Caxivitual.Lunacub.Tests;

public sealed class SimpleResource {
    public int Value { get; set; }
}

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value { get; set; }
}

public sealed class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(stream)!;
    }
}

public sealed class SimpleResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(SimpleResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(SimpleResourceDeserializer);
        
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            
            writer.Write(((SimpleResourceDTO)SerializingObject).Value);
        }
    }
}

public sealed class SimpleResourceDeserializer : Deserializer<SimpleResource> {
    protected override SimpleResource Deserialize(Stream dataStream, Stream optionsStream, DeserializationContext context) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        return new() {
            Value = reader.ReadInt32(),
        };
    }
}
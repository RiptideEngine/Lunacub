namespace Caxivitual.Lunacub.Tests.Common;

public sealed class SimpleResource {
    public int Value { get; set; }
}

public sealed class SimpleResourceDTO {
    public int Value { get; set; }
}

public sealed class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(sourceStreams.PrimaryStream!)!;
    }
}

public sealed class SimpleResourceSerializerFactory : SerializerFactory<SimpleResourceDTO> {
    protected override Serializer<SimpleResourceDTO> CreateSerializer(SimpleResourceDTO serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<SimpleResourceDTO> {
        public override string DeserializerName => nameof(SimpleResourceDeserializer);
        
        public SerializerCore(SimpleResourceDTO serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            
            writer.Write(SerializingObject.Value);
        }
    }
}

public sealed class SimpleResourceDeserializer : Deserializer<SimpleResource> {
    protected override Task<SimpleResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        return Task.FromResult(new SimpleResource { Value = reader.ReadInt32() });
    }
}
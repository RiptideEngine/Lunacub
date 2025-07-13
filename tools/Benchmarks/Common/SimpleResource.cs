namespace Benchmarks.Common;

public sealed class SimpleResource {
    public int Value { get; set; }
}

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value { get; set; }
}

public sealed class SimpleResourceImporter : Importer<SimpleResourceDTO> {
    protected override SimpleResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<SimpleResourceDTO>(sourceStreams.PrimaryStream!)!;
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
    protected override Task<SimpleResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        return Task.FromResult(new SimpleResource { Value = reader.ReadInt32() });
    }
}
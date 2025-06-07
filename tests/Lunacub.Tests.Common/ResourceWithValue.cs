namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ResourceWithValue {
    public int Value { get; set; }
}

public sealed class ResourceWithValueDTO : ContentRepresentation {
    public int Value { get; set; }
}

public sealed class ResourceWithValueImporter : Importer<ResourceWithValueDTO> {
    protected override ResourceWithValueDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<ResourceWithValueDTO>(stream)!;
    }
}

public sealed class ResourceWithValueSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ResourceWithValueDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ResourceWithValueDeserializer);
        
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            
            writer.Write(((ResourceWithValueDTO)SerializingObject).Value);
        }
    }
}

public sealed class ResourceWithValueDeserializer : Deserializer<ResourceWithValue> {
    protected override Task<ResourceWithValue> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        return Task.FromResult(new ResourceWithValue { Value = reader.ReadInt32() });
    }
}
namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ReferenceResource : IDisposable {
    public ReferenceResource? Reference { get; set; }
    public int Value { get; set; }
    public bool Disposed { get; private set; }

    public void Dispose() {
        Disposed = true;
    }
}

public sealed class ReferenceResourceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}

public sealed class ReferenceResourceImporter : Importer<ReferenceResourceDTO> {
    protected override ReferenceResourceDTO Import(Stream stream, ImportingContext context) {
        var dto = JsonSerializer.Deserialize<ReferenceResourceDTO>(stream)!;
        context.AddReference(dto.Reference);

        return dto;
    }
}

public sealed class ReferenceResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ReferenceResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ReferenceResourceDeserializer);

        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            ReferenceResourceDTO serializing = (ReferenceResourceDTO)SerializingObject;
            
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
        
            writer.Write(serializing.Reference);
            writer.Write(serializing.Value);
        }
    }
}

public sealed class ReferenceResourceDeserializer : Deserializer<ReferenceResource> {
    protected override Task<ReferenceResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestReference<ReferenceResource>(nameof(ReferenceResource.Reference), reader.ReadResourceID());
        
        return Task.FromResult(new ReferenceResource { Value = reader.ReadInt32() });
    }

    protected override void ResolveDependencies(ReferenceResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ReferenceResource>(nameof(ReferenceResource.Reference));
    }
}
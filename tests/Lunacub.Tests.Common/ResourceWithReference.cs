namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ResourceWithReference : IDisposable {
    public ResourceWithReference? Reference { get; set; }
    public int Value { get; set; }
    public bool Disposed { get; private set; }

    public void Dispose() {
        Disposed = true;
    }
}

public sealed class ResourceWithReferenceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}

public sealed class ResourceWithReferenceImporter : Importer<ResourceWithReferenceDTO> {
    protected override ResourceWithReferenceDTO Import(Stream resourceStream, ImportingContext context) {
        var dto = JsonSerializer.Deserialize<ResourceWithReferenceDTO>(resourceStream)!;
        context.AddReference(dto.Reference);

        return dto;
    }
}

public sealed class ResourceWithReferenceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ResourceWithReferenceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ResourceWithReferenceDeserializer);

        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            ResourceWithReferenceDTO serializing = (ResourceWithReferenceDTO)SerializingObject;
            
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
        
            writer.Write(serializing.Reference);
            writer.Write(serializing.Value);
        }
    }
}

public sealed class ResourceWithReferenceDeserializer : Deserializer<ResourceWithReference> {
    protected override Task<ResourceWithReference> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestReference<ResourceWithReference>(1, reader.ReadResourceID());
        
        return Task.FromResult(new ResourceWithReference { Value = reader.ReadInt32() });
    }

    protected override void ResolveReferences(ResourceWithReference instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ResourceWithReference>(1);
    }
}
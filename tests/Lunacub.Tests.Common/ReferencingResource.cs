namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ReferencingResource : IDisposable {
    public ReferencingResource? Reference { get; set; }
    public int Value { get; set; }
    public bool Disposed { get; private set; }

    public void Dispose() {
        Disposed = true;
    }
}

public sealed class ReferencingResourceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}

public sealed class ReferencingResourceImporter : Importer<ReferencingResourceDTO> {
    protected override ReferencingResourceDTO Import(Stream resourceStream, ImportingContext context) {
        return JsonSerializer.Deserialize<ReferencingResourceDTO>(resourceStream)!;
    }
}

public sealed class ReferencingResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ReferencingResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ReferencingResourceDeserializer);

        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            ReferencingResourceDTO serializing = (ReferencingResourceDTO)SerializingObject;
            
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
        
            writer.Write(serializing.Reference);
            writer.Write(serializing.Value);
        }
    }
}

public sealed class ReferencingResourceDeserializer : Deserializer<ReferencingResource> {
    protected override Task<ReferencingResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestReference<ReferencingResource>(0, reader.ReadResourceID());
        
        return Task.FromResult(new ReferencingResource { Value = reader.ReadInt32() });
    }

    protected override void ResolveReferences(ReferencingResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ReferencingResource>(0);
    }
}
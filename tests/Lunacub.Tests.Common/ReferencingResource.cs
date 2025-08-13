namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ReferencingResource : IDisposable {
    public ReferencingResource? Reference { get; set; }

    public int Value { get; set; }
    public bool Disposed { get; private set; }

    public void Dispose() {
        Disposed = true;
    }
}

public sealed class ReferencingResourceDTO {
    public ResourceAddress ReferenceAddress { get; set; }
    public int Value { get; set; }
}

public sealed class ReferencingResourceImporter : Importer<ReferencingResourceDTO> {
    protected override ReferencingResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return JsonSerializer.Deserialize<ReferencingResourceDTO>(sourceStreams.PrimaryStream!)!;
    }
}

public sealed class ReferencingResourceSerializerFactory : SerializerFactory<ReferencingResourceDTO> {
    protected override Serializer<ReferencingResourceDTO> CreateSerializer(ReferencingResourceDTO serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<ReferencingResourceDTO> {
        public override string DeserializerName => nameof(ReferencingResourceDeserializer);

        public SerializerCore(ReferencingResourceDTO obj, SerializationContext context) : base(obj, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
        
            writer.Write(SerializingObject.ReferenceAddress);
            writer.Write(SerializingObject.Value);
        }
    }
}

public sealed class ReferencingResourceDeserializer : Deserializer<ReferencingResource> {
    protected override Task<ReferencingResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestingReferences.Add(1, new(reader.ReadResourceAddress()));

        return Task.FromResult(new ReferencingResource { Value = reader.ReadInt32() });
    }

    protected override void ResolveReferences(ReferencingResource instance, DeserializationContext context) {
        if (context.RequestingReferences.TryGetReference(1, out var handle)) {
            if (handle.Value is ReferencingResource reference) {
                instance.Reference = reference;
            } else {
                context.ReleaseReference(1);
            }
        }
    }
}
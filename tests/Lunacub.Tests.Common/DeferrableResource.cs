namespace Caxivitual.Lunacub.Tests.Common;

public sealed class DeferrableResource;

public sealed class DeferrableResourceDTO;

public sealed class DeferrableResourceImporter : Importer<DeferrableResourceDTO> {
    protected override DeferrableResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return new();
    }
}

public sealed class DeferrableResourceSerializerFactory : SerializerFactory<DeferrableResourceDTO> {
    protected override Serializer<DeferrableResourceDTO> CreateSerializer(DeferrableResourceDTO serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<DeferrableResourceDTO> {
        public override string DeserializerName => nameof(DeferrableResourceDeserializer);
        
        public SerializerCore(DeferrableResourceDTO serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
        }
    }
}

public sealed class DeferrableResourceDeserializer : Deserializer<DeferrableResource> {
    private bool _signal;
    
    protected override Task<DeferrableResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        // Probably shouldn't do this but hey it's just a test.
        while (!_signal) {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return Task.FromResult(new DeferrableResource());
    }

    public void Signal() => _signal = true;
}
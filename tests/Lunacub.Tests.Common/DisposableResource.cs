namespace Caxivitual.Lunacub.Tests.Common;

public class DisposableResource : IDisposable {
    public bool Disposed { get; private set; }
    
    public void Dispose() {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}

public sealed class DisposableResourceDTO;

public sealed class DisposableResourceImporter : Importer<DisposableResourceDTO> {
    protected override DisposableResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return new();
    }
}

public sealed class DisposableResourceSerializerFactory : SerializerFactory<DisposableResourceDTO> {
    protected override Serializer<DisposableResourceDTO> CreateSerializer(object serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<DisposableResourceDTO> {
        public override string DeserializerName => nameof(DisposableResourceDeserializer);

        public SerializerCore(object obj, SerializationContext context) : base(obj, context) { }

        public override void SerializeObject(Stream outputStream) { }
    }
}

public sealed class DisposableResourceDeserializer : Deserializer<DisposableResource> {
    protected override Task<DisposableResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        return Task.FromResult(new DisposableResource());
    }
}
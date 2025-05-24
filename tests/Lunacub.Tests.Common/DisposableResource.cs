namespace Caxivitual.Lunacub.Tests.Common;

public class DisposableResource : IDisposable {
    public bool Disposed { get; private set; }
    
    public void Dispose() {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}

public sealed class DisposableResourceDTO : ContentRepresentation;

public sealed class DisposableResourceImporter : Importer<DisposableResourceDTO> {
    protected override DisposableResourceDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<DisposableResourceDTO>(stream)!;
    }
}

public sealed class DisposableResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(DisposableResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(DisposableResourceDeserializer);

        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) { }
    }
}

public sealed class DisposableResourceDeserializer : Deserializer<DisposableResource> {
    protected override Task<DisposableResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        return Task.FromResult(new DisposableResource());
    }
}
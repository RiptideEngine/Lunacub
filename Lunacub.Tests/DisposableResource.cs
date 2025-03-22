namespace Caxivitual.Lunacub.Tests;

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

public sealed class DisposableResourceSerializer : Serializer<DisposableResourceDTO> {
    public override string DeserializerName => nameof(DisposableResourceDeserializer);

    protected override void Serialize(DisposableResourceDTO input, Stream stream) {
    }
}

public sealed class DisposableResourceDeserializer : Deserializer<DisposableResource> {
    protected override DisposableResource Deserialize(Stream stream, DeserializationContext context) {
        return new();
    }
}
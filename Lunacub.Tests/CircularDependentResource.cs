namespace Caxivitual.Lunacub.Tests;

public sealed class CircularDependentResource {
    public CircularDependentResource? Reference { get; set; }
    public int Value { get; set; }
}

public sealed class CircularDependentResourceDTO : ContentRepresentation {
    [JsonPropertyName("Reference")] public ResourceID Reference { get; set; }
    public int Value { get; set; }
}

public sealed class CircularDependentResourceImporter : Importer<CircularDependentResourceDTO> {
    protected override CircularDependentResourceDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<CircularDependentResourceDTO>(stream)!;
    }
}

public sealed class CircularDependentResourceSerializer : Serializer<CircularDependentResourceDTO> {
    public override string DeserializerName => nameof(CircularDependentResourceDeserializer);

    protected override void Serialize(CircularDependentResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(input.Reference);
        writer.Write(input.Value);
    }
}

public sealed class CircularDependentResourceDeserializer : Deserializer<CircularDependentResource> {
    protected override CircularDependentResource Deserialize(Stream stream, DeserializationContext context) {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        
        reader.BaseStream.Seek(4, SeekOrigin.Current);

        context.RequestReference<CircularDependentResource>(nameof(CircularDependentResource.Reference), reader.ReadResourceID());
        
        return new() {
            Value = reader.ReadInt32(),
        };
    }

    protected override void ResolveDependencies(CircularDependentResource instance, DeserializationContext context) {
        instance.Reference = context.GetDependency<CircularDependentResource>(nameof(CircularDependentResource.Reference));
    }
}
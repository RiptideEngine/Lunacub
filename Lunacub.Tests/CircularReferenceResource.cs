namespace Caxivitual.Lunacub.Tests;

public sealed class CircularReferenceResource {
    public CircularReferenceResource? Reference { get; set; }
    public int Value { get; set; }
}

public sealed class CircularReferenceResourceDTO : ContentRepresentation {
    [JsonPropertyName("Reference")] public ResourceID Reference { get; set; }
    public int Value { get; set; }
}

public sealed class CircularReferenceResourceImporter : Importer<CircularReferenceResourceDTO> {
    protected override CircularReferenceResourceDTO Import(Stream stream, ImportingContext context) {
        var dto = JsonSerializer.Deserialize<CircularReferenceResourceDTO>(stream)!;
        context.SetReference(dto.Reference, ResourceReferenceType.Reference);

        return dto;
    }
}

public sealed class CircularReferenceResourceSerializer : Serializer<CircularReferenceResourceDTO> {
    public override string DeserializerName => nameof(CircularReferenceResourceDeserializer);

    protected override void Serialize(CircularReferenceResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(input.Reference);
        writer.Write(input.Value);
    }
}

public sealed class CircularReferenceResourceDeserializer : Deserializer<CircularReferenceResource> {
    protected override CircularReferenceResource Deserialize(Stream stream, DeserializationContext context) {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        
        reader.BaseStream.Seek(4, SeekOrigin.Current);

        context.RequestReference<CircularReferenceResource>(nameof(CircularReferenceResource.Reference), reader.ReadResourceID());
        
        return new() {
            Value = reader.ReadInt32(),
        };
    }

    protected override void ResolveDependencies(CircularReferenceResource instance, DeserializationContext context) {
        instance.Reference = context.GetDependency<CircularReferenceResource>(nameof(CircularReferenceResource.Reference));
    }
}
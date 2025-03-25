namespace Caxivitual.Lunacub.Tests;

public sealed class ReferenceResource {
    public ReferenceResource? Reference { get; set; }
    public int Value { get; set; }
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

public sealed class ReferenceResourceSerializer : Serializer<ReferenceResourceDTO> {
    public override string DeserializerName => nameof(ReferenceResourceDeserializer);

    protected override void Serialize(ReferenceResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        
        writer.Write(input.Reference);
        writer.Write(input.Value);
    }
}

public sealed class ReferenceResourceDeserializer : Deserializer<ReferenceResource> {
    protected override ReferenceResource Deserialize(Stream stream, DeserializationContext context) {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        
        context.RequestReference<ReferenceResource>(nameof(ReferenceResource.Reference), reader.ReadResourceID());
        
        return new() {
            Value = reader.ReadInt32(),
        };
    }

    protected override void ResolveDependencies(ReferenceResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ReferenceResource>(nameof(ReferenceResource.Reference));
    }
}
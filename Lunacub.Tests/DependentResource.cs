namespace Caxivitual.Lunacub.Tests;

public sealed class DependentResource {
    public SimpleResource? Dependency1 { get; set; }
    public SimpleResource? Dependency2 { get; set; }
}

public sealed class DependentResourceDTO : ContentRepresentation {
    [JsonPropertyName("Dependency1")] public ResourceID Dependency1 { get; set; }
    [JsonPropertyName("Dependency2")] public ResourceID Dependency2 { get; set; }
}

public sealed class DependentResourceImporter : Importer<DependentResourceDTO> {
    protected override DependentResourceDTO Import(Stream stream, ImportingContext context) {
        DependentResourceDTO dto = JsonSerializer.Deserialize<DependentResourceDTO>(stream)!;
        context.SetReference(dto.Dependency1, ResourceReferenceType.Dependency);
        context.SetReference(dto.Dependency2, ResourceReferenceType.Dependency);

        return dto;
    }
}

public sealed class DependentResourceSerializer : Serializer<DependentResourceDTO> {
    public override string DeserializerName => nameof(DependentResourceDeserializer);

    protected override void Serialize(DependentResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(input.Dependency1);
        writer.Write(input.Dependency2);
    }
}

public sealed class DependentResourceDeserializer : Deserializer<DependentResource> {
    protected override DependentResource Deserialize(Stream stream, DeserializationContext context) {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        
        reader.BaseStream.Seek(4, SeekOrigin.Current);

        context.RequestReference<SimpleResource>(nameof(DependentResource.Dependency1), reader.ReadResourceID());
        context.RequestReference<SimpleResource>(nameof(DependentResource.Dependency2), reader.ReadResourceID());

        return new();
    }

    protected override void ResolveDependencies(DependentResource instance, DeserializationContext context) {
        instance.Dependency1 = context.GetDependency<SimpleResource>(nameof(DependentResource.Dependency1));
        instance.Dependency2 = context.GetDependency<SimpleResource>(nameof(DependentResource.Dependency2));
    }
}
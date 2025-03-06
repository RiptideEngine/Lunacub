namespace Caxivitual.Lunacub.Tests.Common;

public sealed class DependentResource {
    public SimpleResource? Dependency { get; set; }
}

public sealed class DependentResourceDTO : ContentRepresentation {
    [JsonPropertyName("Dependency")] public ResourceID DependencyRid { get; set; }
}

public sealed class DependentResourceImporter : Importer<DependentResourceDTO> {
    protected override DependentResourceDTO Import(Stream stream) {
        DependentResourceDTO dto = JsonSerializer.Deserialize<DependentResourceDTO>(stream)!;
        dto.Dependencies.Add(dto.DependencyRid);

        return dto;
    }
}

public sealed class DependentResourceSerializer : Serializer<DependentResourceDTO> {
    public override string DeserializerName => nameof(DependentResourceDeserializer);

    protected override void Serialize(DependentResourceDTO input, Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(input.DependencyRid);
    }
}

public sealed class DependentResourceDeserializer : Deserializer<DependentResource> {
    protected override DependentResource Deserialize(Stream stream, DeserializationContext context) {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);

        context.RequestDependency<SimpleResource>(nameof(DependentResource.Dependency), reader.ReadResourceID());

        return new();
    }

    protected override void ResolveDependencies(DependentResource instance, DeserializationContext context) {
        instance.Dependency = context.GetDependency<SimpleResource>(nameof(DependentResource.Dependency));
    }
}
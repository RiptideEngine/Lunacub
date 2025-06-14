using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ResourceWithDependencies {
    public List<ResourceWithDependencies> Dependencies { get; set; } = [];
    public int Value { get; set; }
}

public sealed class ResourceWithDependenciesDTO : ContentRepresentation {
    public List<ResourceID> Dependencies { get; set; } = [];
    public int Value { get; set; }
}

public sealed class ResourceWithDependenciesImporter : Importer<ResourceWithDependenciesDTO> {
    public override IReadOnlyCollection<ResourceID> GetDependencies(Stream stream) {
        // Lazy implementation
        return JsonSerializer.Deserialize<JsonObject>(stream)?[nameof(ResourceWithDependenciesDTO.Dependencies)]?.AsArray().Deserialize<ResourceID[]>() ?? [];
    }

    protected override ResourceWithDependenciesDTO Import(Stream resourceStream, ImportingContext context) {
        ResourceWithDependenciesDTO imported = JsonSerializer.Deserialize<ResourceWithDependenciesDTO>(resourceStream)!;

        foreach (var dependency in imported.Dependencies) {
            context.AddReference(dependency);
        }

        return imported;
    }
}

public sealed class ResourceWithDependenciesProcessor : Processor<ResourceWithDependenciesDTO, ResourceWithDependenciesDTO> {
    protected override ResourceWithDependenciesDTO Process(ResourceWithDependenciesDTO importedObject, ProcessingContext context) {
        return importedObject;
    }
}

public sealed class ResourceWithDependenciesSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ResourceWithDependenciesDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ResourceWithDependenciesDeserializer);
        
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            var dto = (ResourceWithDependenciesDTO)SerializingObject;
            
            writer.Write7BitEncodedInt(dto.Dependencies.Count);
            writer.WriteReinterpret<ResourceID>(CollectionsMarshal.AsSpan(dto.Dependencies));
            writer.Write(dto.Value);
        }
    }
}

public sealed class ResourceWithDependenciesDeserializer : Deserializer<ResourceWithDependencies> {
    protected override unsafe Task<ResourceWithDependencies> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int count = reader.Read7BitEncodedInt();
        dataStream.Seek(sizeof(ResourceID) * count, SeekOrigin.Current);

        ResourceID[] dependencyIds = new ResourceID[count];
        reader.ReadReinterpret<ResourceID>(dependencyIds);

        IEnumerable<(int, ResourceID)> dependencyOrder = dependencyIds.Index();
        context.ValueContainer.Add("DependencyOrder", dependencyOrder);

        foreach ((int index, ResourceID dependencyId) in dependencyOrder) {
            context.RequestReference<ResourceWithDependencies>((uint)index, dependencyId);
        }
        
        return Task.FromResult(new ResourceWithDependencies {
            Value = reader.ReadInt32(),
            Dependencies = new(count),
        });
    }

    protected override void ResolveReferences(ResourceWithDependencies instance, DeserializationContext context) {
        foreach ((int index, _) in (IEnumerable<(int, ResourceID)>)context.ValueContainer["DependencyOrder"]) {
            if (context.GetReference<ResourceWithDependencies>((uint)index) is not { } dependency) continue;
            
            instance.Dependencies.Add(dependency);
        }
    }
}
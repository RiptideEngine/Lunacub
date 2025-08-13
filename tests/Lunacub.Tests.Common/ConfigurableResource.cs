using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ConfigurableResource {
    public ImmutableArray<int> Array { get; }

    public ConfigurableResource(ImmutableArray<int> array) {
        Array = array;
    }
}

public enum OutputType {
    Json,
    Binary,
}

public sealed class ConfigurableResourceDTO {
    public ImmutableArray<int> Array { get; }

    public ConfigurableResourceDTO(ImmutableArray<int> array) {
        Array = array;
    }

    public record Options(OutputType OutputType) : IImportOptions;
}

public sealed class ConfigurableResourceImporter : Importer<ConfigurableResourceDTO> {
    protected override ConfigurableResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
        return new(JsonSerializer.Deserialize<ImmutableArray<int>>(sourceStreams.PrimaryStream!));
    }
}

public sealed class ConfigurableResourceSerializerFactory : SerializerFactory<ConfigurableResourceDTO> {
    protected override Serializer<ConfigurableResourceDTO> CreateSerializer(ConfigurableResourceDTO serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<ConfigurableResourceDTO> {
        public override string DeserializerName => nameof(ConfigurableResourceDeserializer);

        public SerializerCore(ConfigurableResourceDTO serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            var buffer = SerializingObject.Array;

            switch (((ConfigurableResourceDTO.Options)Context.Options!).OutputType) {
                case OutputType.Json:
                    JsonSerializer.Serialize(outputStream, buffer);
                    return;
                
                case OutputType.Binary:
                    using (BinaryWriter bw = new BinaryWriter(outputStream, Encoding.UTF8, true)) {
                        foreach (var item in buffer) {
                            bw.Write(item);
                        }
                    }
                    break;
                
                default: throw new UnreachableException();
            }
        }

        public override void SerializeOptions(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, leaveOpen: true);
            
            writer.Write((int)((ConfigurableResourceDTO.Options)Context.Options!).OutputType);
        }
    }
}

public sealed class ConfigurableResourceDeserializer : Deserializer<ConfigurableResource> {
    protected override async Task<ConfigurableResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using BinaryReader optionsReader = new(optionStream, Encoding.UTF8, leaveOpen: true);
        OutputType outputType = (OutputType)optionsReader.ReadInt32();

        switch (outputType) {
            case Common.OutputType.Json:
            {
                var buffer = await JsonSerializer.DeserializeAsync<ImmutableArray<int>>(dataStream, cancellationToken: cancellationToken);
                return new(buffer);
            }

            case Common.OutputType.Binary:
            {
                Debug.Assert(dataStream.Length % 4 == 0);

                byte[] buffer = new byte[dataStream.Length];
                await dataStream.ReadExactlyAsync(buffer, cancellationToken);

                return new([..MemoryMarshal.Cast<byte, int>(buffer)]);
            }

            default: throw new NotSupportedException("Unsupported serialization type.");
        }
    }
}
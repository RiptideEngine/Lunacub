using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ResourceWithOptions {
    public ImmutableArray<int> Array { get; }

    public ResourceWithOptions(ImmutableArray<int> array) {
        Array = array;
    }
}

public enum OutputType {
    Json,
    Binary,
}

public sealed class ResourceWithOptionsDTO : ContentRepresentation {
    public ImmutableArray<int> Array { get; }

    public ResourceWithOptionsDTO(ImmutableArray<int> array) {
        Array = array;
    }

    public record Options(OutputType OutputType) : IImportOptions;
}

public sealed class ResourceWithOptionsImporter : Importer<ResourceWithOptionsDTO> {
    protected override ResourceWithOptionsDTO Import(Stream resourceStream, ImportingContext context) {
        return new(JsonSerializer.Deserialize<ImmutableArray<int>>(resourceStream));
    }
}

public sealed class ResourceWithOptionsSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ResourceWithOptionsDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ResourceWithOptionsDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            var buffer = ((ResourceWithOptionsDTO)SerializingObject).Array;

            switch (((ResourceWithOptionsDTO.Options)Context.Options!).OutputType) {
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
            
            writer.Write((int)((ResourceWithOptionsDTO.Options)Context.Options!).OutputType);
        }
    }
}

public sealed class ResourceWithOptionsDeserializer : Deserializer<ResourceWithOptions> {
    protected override async Task<ResourceWithOptions> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
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
using System.Collections.Immutable;
using System.Diagnostics;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class OptionsResource {
    public ImmutableArray<int> Array { get; }

    public OptionsResource(ImmutableArray<int> array) {
        Array = array;
    }
}

public enum OutputType {
    Json,
    Binary,
}

public sealed class OptionsResourceDTO : ContentRepresentation {
    public ImmutableArray<int> Array { get; }

    public OptionsResourceDTO(ImmutableArray<int> array) {
        Array = array;
    }

    public record Options(OutputType OutputType) : IImportOptions;
}

public sealed class OptionsResourceImporter : Importer<OptionsResourceDTO> {
    protected override OptionsResourceDTO Import(Stream stream, ImportingContext context) {
        return new(JsonSerializer.Deserialize<ImmutableArray<int>>(stream));
    }
}

public sealed class OptionsResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(OptionsResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(OptionsResourceDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            var buffer = ((OptionsResourceDTO)SerializingObject).Array;

            switch (((OptionsResourceDTO.Options)Context.Options!).OutputType) {
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
            
            writer.Write((int)((OptionsResourceDTO.Options)Context.Options!).OutputType);
        }
    }
}

public sealed class OptionsResourceDeserializer : Deserializer<OptionsResource> {
    protected override OptionsResource Deserialize(Stream dataStream, Stream optionStream, DeserializationContext context) {
        using BinaryReader optionsReader = new(optionStream, Encoding.UTF8, leaveOpen: true);
        OutputType outputType = (OutputType)optionsReader.ReadInt32();

        switch (outputType) {
            case Common.OutputType.Json:
                return new(JsonSerializer.Deserialize<ImmutableArray<int>>(dataStream));
            
            case Common.OutputType.Binary:
                Debug.Assert(dataStream.Length % 4 == 0);

                using (BinaryReader dataReader = new(dataStream, Encoding.UTF8, leaveOpen: true)) {
                    var builder = ImmutableArray.CreateBuilder<int>((int)(dataStream.Length / 4));

                    for (int i = 0, e = (int)(dataStream.Length / 4); i < e; i++) {
                        builder.Add(dataReader.ReadInt32());
                    }

                    return new(builder.MoveToImmutable());
                }
                
            default: throw new NotSupportedException("Unsupported serialization type.");
        }
    }
}
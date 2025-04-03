using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace Caxivitual.Lunacub.Tests;

public sealed class OptionsResource {
    public ImmutableArray<int> Array { get; }

    public OptionsResource(ImmutableArray<int> array) {
        Array = array;
    }
}

public enum SerializationType {
    Json,
    Binary,
}

public sealed class OptionsResourceDTO : ContentRepresentation {
    public ImmutableArray<int> Array { get; }

    public OptionsResourceDTO(ImmutableArray<int> array) {
        Array = array;
    }

    public record Options(SerializationType SerializationType) : IImportOptions;
}

public sealed class ProcesseedOptionsResourceDTO : ContentRepresentation {
    public byte[] Buffer { get; }

    public ProcesseedOptionsResourceDTO(byte[] buffer) {
        Buffer = buffer;
    }
}

public sealed class OptionsResourceImporter : Importer<OptionsResourceDTO> {
    protected override OptionsResourceDTO Import(Stream stream, ImportingContext context) {
        return new(JsonSerializer.Deserialize<ImmutableArray<int>>(stream));
    }
}

public sealed class OptionsResourceProcessor : Processor<OptionsResourceDTO, ProcesseedOptionsResourceDTO> {
    protected override ProcesseedOptionsResourceDTO Process(OptionsResourceDTO input, ProcessingContext context) {
        var options = (OptionsResourceDTO.Options)context.Options!;
        using MemoryStream ms = new();
        
        switch (options.SerializationType) {
            case SerializationType.Json:
                JsonSerializer.Serialize(ms, input.Array);
                return new(ms.ToArray());
            
            case SerializationType.Binary:
                using (BinaryWriter writer = new(ms)) {
                    foreach (var value in input.Array) {
                        writer.Write(value);
                    }
                }
                return new(ms.ToArray());

            default: throw new NotSupportedException();
        }
    }
}

public sealed class OptionsResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ProcesseedOptionsResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(OptionsResourceDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            outputStream.Write(((ProcesseedOptionsResourceDTO)SerializingObject).Buffer);
        }

        public override void SerializeOptions(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, leaveOpen: true);
            
            writer.Write((int)((OptionsResourceDTO.Options)Context.Options!).SerializationType);
        }
    }
}

public sealed class OptionsResourceDeserializer : Deserializer<OptionsResource> {
    protected override OptionsResource Deserialize(Stream dataStream, Stream optionStream, DeserializationContext context) {
        using BinaryReader optionsReader = new(dataStream, Encoding.UTF8, leaveOpen: true);
        SerializationType serializationType = (SerializationType)optionsReader.ReadInt32();

        switch (serializationType) {
            case SerializationType.Json:
                return new(JsonSerializer.Deserialize<ImmutableArray<int>>(dataStream));
            
            case SerializationType.Binary:
                Debug.Assert(dataStream.Length % 4 == 0);

                using (BinaryReader dataReader = new(dataStream, Encoding.UTF8, leaveOpen: true)) {
                    var builder = ImmutableArray.CreateBuilder<int>();

                    for (int i = 0, e = (int)(dataStream.Length / 4); i < e; i++) {
                        builder.Add(dataReader.ReadInt32());
                    }

                    return new(builder.MoveToImmutable());
                }
                
            default: throw new NotSupportedException();
        }
    }
}
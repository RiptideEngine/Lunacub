using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Importing;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebGpuSampler = Silk.NET.WebGPU.Sampler;

namespace Lunacub.Playground;

public sealed unsafe class Sampler : IDisposable {
    public WebGpuSampler* Handle { get; private set; }

    private readonly RenderingSystem _renderingSystem;

    public Sampler(RenderingSystem renderingSystem, SamplerDescriptor descriptor) {
        Handle = renderingSystem.WebGPU.DeviceCreateSampler(renderingSystem.RenderingDevice.Device, &descriptor);
        
        _renderingSystem = renderingSystem;
    }

    public void Dispose() {
        if (Handle == null) return;
        
        _renderingSystem.WebGPU.SamplerRelease(Handle);
        Handle = null;
    }
}

public sealed class SamplerDTO : ContentRepresentation {
    public AddressMode AddressU { get; set; }
    public AddressMode AddressV { get; set; }
    public AddressMode AddressW { get; set; }
    public FilterMode MinFilter { get; set; }
    public FilterMode MagFilter { get; set; }
    public MipmapFilterMode MipmapFilter { get; set; }
    public float LodMinClamp { get; set; }
    public float LodMaxClamp { get; set; } = float.MaxValue;
    public CompareFunction Comparison { get; set; }
    public ushort MaxAnisotropy { get; set; }
}

public sealed class SamplerImporter : Importer<SamplerDTO> {
    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerOptions.Default) {
        Converters = {
            new JsonStringEnumConverter(),
        },
    };
    
    protected override SamplerDTO Import(Stream stream, ImportingContext context) {
        return JsonSerializer.Deserialize<SamplerDTO>(stream, jsonOptions) ?? throw new("Failed to deserialize sampler resource data.");
    }
}

public sealed class SamplerSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(SamplerDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(SamplerDeserializer);

        public SerializerCore(ContentRepresentation serializationObject, SerializationContext context) : base(serializationObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            SamplerDTO serializing = (SamplerDTO)SerializingObject;
            
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);

            writer.Write((byte)serializing.AddressU);
            writer.Write((byte)serializing.AddressV);
            writer.Write((byte)serializing.AddressW);
            writer.Write((byte)serializing.MinFilter);
            writer.Write((byte)serializing.MagFilter);
            writer.Write((byte)serializing.MipmapFilter);
            writer.Write(serializing.LodMinClamp);
            writer.Write(serializing.LodMaxClamp);
            writer.Write((byte)serializing.Comparison);
            writer.Write(serializing.MaxAnisotropy);
        }
    }
}

public sealed class SamplerDeserializer : Deserializer<Sampler> {
    private readonly RenderingSystem _renderingSystem;
    
    public SamplerDeserializer(RenderingSystem renderingSystem) {
        _renderingSystem = renderingSystem;
    }
    
    protected override Sampler Deserialize(Stream stream, Stream optionsStream, DeserializationContext context) {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        
        return new(_renderingSystem, new() {
            AddressModeU = (AddressMode)reader.ReadByte(),
            AddressModeV = (AddressMode)reader.ReadByte(),
            AddressModeW = (AddressMode)reader.ReadByte(),
            MinFilter = (FilterMode)reader.ReadByte(),
            MagFilter = (FilterMode)reader.ReadByte(),
            MipmapFilter = (MipmapFilterMode)reader.ReadByte(),
            LodMinClamp = reader.ReadSingle(),
            LodMaxClamp = reader.ReadSingle(),
            Compare = (CompareFunction)reader.ReadByte(),
            MaxAnisotropy = reader.ReadUInt16(),
        });
    }
}
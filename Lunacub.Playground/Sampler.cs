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

public sealed class SamplerSerializer : Serializer<SamplerDTO> {
    public override string DeserializerName => nameof(SamplerDeserializer);

    protected override void Serialize(SamplerDTO input, Stream stream) {
        using BinaryWriter writer = new(stream, Encoding.UTF8, true);
        
        writer.Write((byte)input.AddressU);
        writer.Write((byte)input.AddressV);
        writer.Write((byte)input.AddressW);
        writer.Write((byte)input.MinFilter);
        writer.Write((byte)input.MagFilter);
        writer.Write((byte)input.MipmapFilter);
        writer.Write(input.LodMinClamp);
        writer.Write(input.LodMaxClamp);
        writer.Write((byte)input.Comparison);
        writer.Write(input.MaxAnisotropy);
    }
}

public sealed class SamplerDeserializer : Deserializer<Sampler> {
    private readonly RenderingSystem _renderingSystem;
    
    public SamplerDeserializer(RenderingSystem renderingSystem) {
        _renderingSystem = renderingSystem;
    }
    
    protected override Sampler Deserialize(Stream stream, DeserializationContext context) {
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
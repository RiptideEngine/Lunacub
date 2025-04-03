using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Importing;
using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Lunacub.Playground;

public sealed unsafe class Texture2D : IDisposable {
    public Texture* Handle { get; private set; }
    public TextureView* View { get; private set; }

    public int Width => (int)_renderingSystem.WebGPU.TextureGetWidth(Handle);
    public int Height => (int)_renderingSystem.WebGPU.TextureGetHeight(Handle);

    private readonly RenderingSystem _renderingSystem;

    public Texture2D(RenderingSystem renderingSystem, int width, int height) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Handle = renderingSystem.WebGPU.DeviceCreateTexture(renderingSystem.RenderingDevice.Device, new TextureDescriptor {
            Size = new() { Width = (uint)width, Height = (uint)height, DepthOrArrayLayers = 1 },
            Dimension = TextureDimension.Dimension2D,
            Format = TextureFormat.Rgba8Unorm,
            MipLevelCount = 1,
            SampleCount = 1,
            ViewFormats = null,
            ViewFormatCount = 0,
            NextInChain = null,
            Usage = TextureUsage.CopySrc | TextureUsage.CopyDst | TextureUsage.TextureBinding,
        });

        View = renderingSystem.WebGPU.TextureCreateView(Handle, new TextureViewDescriptor {
            Dimension = TextureViewDimension.Dimension2D,
            Aspect = TextureAspect.All,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            Format = TextureFormat.Rgba8Unorm,
        });
        
        _renderingSystem = renderingSystem;
    }

    public void Dispose() {
        if (Handle == null) return;
        
        _renderingSystem.WebGPU.TextureRelease(Handle);
        Handle = null;
        
        _renderingSystem.WebGPU.TextureViewRelease(View);
        View = null;
    }
}

public sealed class Texture2DDTO : ContentRepresentation {
    public Image Image { get; }

    internal Texture2DDTO(Image image) {
        Image = image;
    }
    
    protected override void DisposeImpl(bool disposing) {
        Image.Dispose();
    }
}

public sealed class Texture2DImporter : Importer<Texture2DDTO> {
    protected override Texture2DDTO Import(Stream stream, ImportingContext context) {
        return new(Image.Load(stream));
    }
}

public sealed class Texture2DSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(Texture2DDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(Texture2DDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            ((Texture2DDTO)SerializingObject).Image.SaveAsQoi(outputStream);
        }
    }
}

public sealed class Texture2DDeserializer : Deserializer<Texture2D> {
    private readonly RenderingSystem _renderingSystem;
    
    public Texture2DDeserializer(RenderingSystem renderingSystem) {
        _renderingSystem = renderingSystem;
    }
    
    protected unsafe override Texture2D Deserialize(Stream stream, Stream optionsStream, DeserializationContext context) {
        using Image<Rgba32> image = Image.Load<Rgba32>(stream);
        Texture2D output = new(_renderingSystem, image.Width, image.Height);
        
        image.ProcessPixelRows(accessor => {
            var wgpuQueue = _renderingSystem.RenderingDevice.Queue;
            ImageCopyTexture destination = new() {
                Aspect = TextureAspect.All,
                MipLevel = 0,
                Texture = output.Handle,
            };
            
            nuint rowSize = (nuint)sizeof(Rgba32) * (nuint)accessor.Width;
            
            TextureDataLayout layout = new() {
                BytesPerRow = (uint)rowSize,
                Offset = 0,
                RowsPerImage = (uint)accessor.Height,
            };
            
            Extent3D writeSize = new() {
                Width = (uint)accessor.Width,
                Height = 1,
                DepthOrArrayLayers = 1,
            };
            
            for (int y = 0; y < accessor.Height; y++) {
                destination.Origin = new() {
                    X = 0,
                    Y = (uint)y,
                    Z = 0,
                };
                
                fixed (Rgba32* pixels = accessor.GetRowSpan(y)) {
                    _renderingSystem.WebGPU.QueueWriteTexture(wgpuQueue, &destination, pixels, rowSize, &layout, &writeSize);
                }
            }
        });

        return output;
    }
}
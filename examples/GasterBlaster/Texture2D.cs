using Silk.NET.WebGPU;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed unsafe class Texture2D : BaseDisposable {
    private readonly Renderer _renderer;

    public Texture* Texture { get; private set; }
    public TextureView* View { get; private set; }

    public Texture2D(Renderer renderer, uint width, uint height, TextureFormat format) {
        _renderer = renderer;
        
        Texture = _renderer.WebGPU.DeviceCreateTexture(_renderer.RenderingDevice.Device, new TextureDescriptor {
            Usage = TextureUsage.TextureBinding | TextureUsage.CopyDst | TextureUsage.CopySrc,
            Size = new() { Width = width, Height = height, DepthOrArrayLayers = 1 },
            Dimension = TextureDimension.Dimension2D,
            Format = format,
            MipLevelCount = 1,
            SampleCount = 1,
        });
        
        View = _renderer.WebGPU.TextureCreateView(Texture, new TextureViewDescriptor {
            Dimension = TextureViewDimension.Dimension2D,
            Aspect = TextureAspect.All,
            Format = format,
            ArrayLayerCount = 1,
            BaseArrayLayer = 0,
            MipLevelCount = 1,
            BaseMipLevel = 0,
        });
    }

    protected override void DisposeImpl(bool disposing) {
        if (View != null) {
            _renderer.WebGPU.TextureViewRelease(View);
            View = null;
        }

        if (Texture != null) {
            _renderer.WebGPU.TextureRelease(Texture);
            Texture = null;
        }
    }
}
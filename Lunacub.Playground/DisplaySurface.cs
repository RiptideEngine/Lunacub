using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;

namespace Lunacub.Playground;

public sealed unsafe class DisplaySurface : IDisposable {
    public const TextureFormat SurfaceFormat = TextureFormat.Bgra8Unorm;

    private readonly RenderingSystem _system;
    
    internal Surface* Surface { get; private set; }

    internal DisplaySurface(RenderingSystem system, IView window) {
        Application.Logger.LogInformation("Initializing WebGPU Surface...");
        
        Surface = window.CreateWebGPUSurface(system.WebGPU, system.Instance);

        if (Surface == null) {
            throw new("Failed to create surface.");
        }
        
        _system = system;
    }

    public void Configurate(uint width, uint height) {
        _system.WebGPU.SurfaceConfigure(Surface, new SurfaceConfiguration {
            AlphaMode = CompositeAlphaMode.Auto,
            Device = _system.RenderingDevice.Device,
            Format = SurfaceFormat,
            Width = width,
            Height = height,
            PresentMode = PresentMode.Fifo,
            Usage = TextureUsage.RenderAttachment,
            ViewFormats = null,
            ViewFormatCount = 0,
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SurfaceTexture GetCurrentSurfaceTexture() {
        SurfaceTexture texture;
        _system.WebGPU.SurfaceGetCurrentTexture(Surface, &texture);

        return texture;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Present() {
        _system.WebGPU.SurfacePresent(Surface);
    }

    private void Dispose(bool disposing) {
        if (Surface == null) return;
        
        _system.WebGPU.SurfaceRelease(Surface);
        Surface = null;
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DisplaySurface() {
        Dispose(false);
    }
}
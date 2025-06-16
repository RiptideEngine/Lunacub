using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using System.Diagnostics;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public sealed unsafe class Renderer : IDisposable {
    private static bool _disposed;
    
    public WebGPU WebGPU { get; private set; }

    internal Instance* Instance { get; private set; }
    
    public RenderingDevice RenderingDevice { get; private set; }
    
    internal Surface* Surface { get; private set; }
    
    public TextureView* CurrentSurfaceView { get; private set; }

    public Renderer(IView outputWindow) {
        WebGPU = WebGPU.GetApi();

        Instance = WebGPU.CreateInstance(new InstanceDescriptor());
        Surface = outputWindow.CreateWebGPUSurface(WebGPU, Instance);

        try {
            RenderingDevice = new(WebGPU, Instance, Surface);
            
            var framebufferSize = outputWindow.FramebufferSize;
            
            ConfigurateSurface((uint)framebufferSize.X, (uint)framebufferSize.Y);
        } catch {
            Dispose();
            throw;
        }
    }
    
    public void ConfigurateSurface(uint width, uint height) {
        Debug.Assert(Surface != null);
        
        WebGPU.SurfaceConfigure(Surface, new SurfaceConfiguration {
            AlphaMode = CompositeAlphaMode.Auto,
            Device = RenderingDevice.Device,
            Format = TextureFormat.Bgra8Unorm,
            Width = width,
            Height = height,
            PresentMode = PresentMode.Fifo,
            Usage = TextureUsage.RenderAttachment,
        });
    }
    
    private SurfaceTexture GetCurrentSurfaceTexture() {
        SurfaceTexture texture;
        WebGPU.SurfaceGetCurrentTexture(Surface, &texture);

        return texture;
    }
    
    public void BeginRenderingFrame() {
        if (CurrentSurfaceView != null) throw new InvalidOperationException("Rendering frame has already started.");

        SurfaceTexture surfaceTexture = GetCurrentSurfaceTexture();

        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success) {
            throw new($"Failed to get surface texture, status '{surfaceTexture.Status}'.");
        }
        
        CurrentSurfaceView = WebGPU.TextureCreateView(surfaceTexture.Texture, null);
    }

    public void Present() {
        if (CurrentSurfaceView == null) return;
        
        WebGPU.TextureViewRelease(CurrentSurfaceView); CurrentSurfaceView = null;
        WebGPU.SurfacePresent(Surface);
    }

    private void Dispose(bool disposing) {
        if (CurrentSurfaceView != null) {
            WebGPU.TextureViewRelease(CurrentSurfaceView);
            CurrentSurfaceView = null;
        }

        if (disposing) {
            RenderingDevice?.Dispose();
        }

        if (Instance != null) {
            WebGPU.InstanceRelease(Instance);
            Instance = null;
        }
        
        WebGPU.SurfaceRelease(Surface);
        Surface = null;
        
        WebGPU.Dispose(); WebGPU = null!;
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Renderer() {
        Dispose(false);
    }
}
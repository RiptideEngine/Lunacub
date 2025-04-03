using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.Windowing;

namespace Lunacub.Playground;

public sealed unsafe class RenderingSystem : IDisposable {
    private bool _disposed;

    public WebGPU WebGPU { get; private set; }
    public Wgpu WGPU { get; private set; }

    internal Instance* Instance { get; private set; }
    
    public DisplaySurface DisplaySurface { get; private set; }
    public RenderingDevice RenderingDevice { get; private set; }
    public TextureView* CurrentSurfaceView { get; private set; }
    
    public RenderingSystem(IWindow window) {
        Application.Logger.LogInformation("Initializing {name}...", GetType().Name);
        
        WebGPU = WebGPU.GetApi();
        WGPU = new(WebGPU.Context);
        
        Application.Logger.LogInformation("Initializing WebGPU Instance...");
        
        InstanceExtras instanceExtras = new() {
            Chain = new() {
                SType = (SType)NativeSType.STypeInstanceExtras,
            },
            Backends = InstanceBackend.Secondary,  // For some reason, DX11 and DX12 is broken (downgrade Silk.NET to 2.20 fixed DX12)
        };

        // string dxcPath = Path.Combine(ShaderingSystem.DxcLibraryDirectory!, "dxcompiler.dll");
        // string dxilPath = Path.Combine(ShaderingSystem.DxcLibraryDirectory!, "dxil.dll");
        //
        // byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(dxcPath) + Encoding.UTF8.GetByteCount(dxilPath) + 2);
        // try {
        //     int dxcPathConvertLength = Encoding.UTF8.GetBytes(dxcPath, buffer);
        //     buffer[dxcPathConvertLength] = 0;
        //     int dxilPathConvertLength = Encoding.UTF8.GetBytes(dxilPath, buffer.AsSpan(dxcPathConvertLength + 1));
        //     buffer[dxcPathConvertLength + 1 + dxilPathConvertLength] = 0;
        //
        //     fixed (byte* paths = buffer) {
        //         instanceExtras.DxcPath = paths;
        //         instanceExtras.DxilPath = paths + dxcPathConvertLength + 1;
        //         instanceExtras.Dx12ShaderCompiler = Dx12Compiler.Dxc;
        //     }
        // } finally {
        //     ArrayPool<byte>.Shared.Return(buffer);
        // }

        Instance = WebGPU.CreateInstance(new InstanceDescriptor {
            NextInChain = &instanceExtras.Chain,
        });

        if (Instance == null) {
            Dispose();
            throw new("Failed to create WebGPU Instance.");
        }
        
        DisplaySurface = new(this, window);
        RenderingDevice = new(WebGPU, Instance, DisplaySurface.Surface);
        
        Application.Logger.LogInformation("Configurating WebGPU Surface...");
        
        var framebufferSize = window.FramebufferSize;
        DisplaySurface.Configurate((uint)framebufferSize.X, (uint)framebufferSize.Y);
    }
    
    public void BeginRenderingFrame() {
        if (CurrentSurfaceView != null) throw new InvalidOperationException("Rendering frame has already started.");

        SurfaceTexture surfaceTexture = DisplaySurface.GetCurrentSurfaceTexture();

        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success) {
            throw new($"Failed to get surface texture, status '{surfaceTexture.Status}'.");
        }
        
        CurrentSurfaceView = WebGPU.TextureCreateView(surfaceTexture.Texture, null);
    }

    public void Present() {
        if (CurrentSurfaceView == null) return;
        
        WebGPU.TextureViewRelease(CurrentSurfaceView); CurrentSurfaceView = null;
        DisplaySurface.Present();
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        
        WebGPU.InstanceRelease(Instance);
        Instance = null;

        if (disposing) {
            DisplaySurface.Dispose();
            DisplaySurface = null!;
            
            RenderingDevice.Dispose();
            RenderingDevice = null!;

            WebGPU.Dispose();
            
            WGPU = null!;
            WebGPU = null!;
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RenderingSystem() {
        Dispose(false);
    }
}
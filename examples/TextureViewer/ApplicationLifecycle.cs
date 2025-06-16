using Silk.NET.WebGPU;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

internal static unsafe class ApplicationLifecycle {
    private static Renderer _renderer = null!;
    
    public static void Initialize() {
        _renderer = new(Application.MainWindow);
    }

    public static void Update() {
        
    }

    public static void Render() {
        _renderer.BeginRenderingFrame();
        
        RenderPassColorAttachment colorAttachment = new() {
            View = _renderer.CurrentSurfaceView,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new() { R = 0, G = 0.2, B = 0.4, A = 0 },
            ResolveTarget = null,
        };
        
        var webgpu = _renderer.WebGPU;
        
        var cmdEncoder = webgpu.DeviceCreateCommandEncoder(_renderer.RenderingDevice.Device, new CommandEncoderDescriptor());
        
        var renderPass = webgpu.CommandEncoderBeginRenderPass(cmdEncoder, new RenderPassDescriptor() {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = null,
        });
        
        webgpu.RenderPassEncoderEnd(renderPass);
        
        var cmdBuffer = webgpu.CommandEncoderFinish(cmdEncoder, new CommandBufferDescriptor());
        
        webgpu.QueueSubmit(_renderer.RenderingDevice.Queue, 1, &cmdBuffer);
        
        webgpu.CommandBufferRelease(cmdBuffer);
        webgpu.CommandEncoderRelease(cmdEncoder);
        
        _renderer.Present();
    }

    public static void Shutdown() {
        _renderer.Dispose();
        _renderer = null!;
    }
}
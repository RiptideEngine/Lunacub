using Silk.NET.WebGPU;
using System.Buffers;
using System.Text;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public sealed unsafe class Shader : BaseDisposable {
    private readonly Renderer _renderer;
    public ShaderModule* Module { get; private set; }

    private Shader(Renderer renderer, ShaderModule* module) {
        _renderer = renderer;
        Module = module;
    }

    public Shader(Renderer renderer, string source) {
        _renderer = renderer;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(source) + 1);
        try {
            buffer[Encoding.UTF8.GetBytes(source, buffer)] = 0;

            fixed (byte* code = buffer) {
                ShaderModuleWGSLDescriptor wgslDescriptor = new() {
                    Chain = new() {
                        SType = SType.ShaderModuleWgslDescriptor,
                    },

                    Code = code,
                };

                Module = _renderer.WebGPU.DeviceCreateShaderModule(_renderer.RenderingDevice.Device, new ShaderModuleDescriptor {
                    NextInChain = &wgslDescriptor.Chain,
                });
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    protected override void DisposeImpl(bool disposing) {
        if (Module != null) {
            _renderer.WebGPU.ShaderModuleRelease(Module);
            Module = null;
        }
    }

    public static Shader FromFile(Renderer renderer, string path) {
        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Shader.wgsl"), FileMode.Open, FileAccess.Read, FileShare.Read)) {
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length + 1);

            try {
                fs.ReadExactly(buffer.AsSpan(0, (int)fs.Length));

                fixed (byte* ptr = buffer) {
                    ShaderModuleWGSLDescriptor wgslDescriptor = new() {
                        Chain = new() {
                            SType = SType.ShaderModuleWgslDescriptor,
                        },

                        Code = ptr,
                    };
                
                    ShaderModule* module = renderer.WebGPU.DeviceCreateShaderModule(renderer.RenderingDevice.Device, new ShaderModuleDescriptor {
                        NextInChain = &wgslDescriptor.Chain,
                    });

                    return new(renderer, module);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
using Silk.NET.WebGPU;
using System.Numerics;
using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

internal static unsafe class ApplicationLifecycle {
    private static Renderer _renderer = null!;

    // Mesh
    private static WebGPUBuffer* _vertexBuffer = null!;
    private static WebGPUBuffer* _indexBuffer = null!;
    
    // Material
    private static ShaderModule* _shaderModule = null!;
    private static BindGroupLayout* _bindGroupLayout = null!;
    private static PipelineLayout* _pipelineLayout = null!;
    private static RenderPipeline* _renderPipeline = null!;
    
    // Drawing
    private static BindGroup* _bindGroup = null!;
    
    public static void Initialize() {
        _renderer = new(Application.MainWindow);

        // Create mesh.
        _vertexBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
            MappedAtCreation = false,
            Size = (ulong)sizeof(Vertex) * 4,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
        });
        _indexBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
            MappedAtCreation = false,
            Size = sizeof(ushort) * 6,
            Usage = BufferUsage.Index | BufferUsage.CopyDst,
        });
        
        _renderer.WebGPU.QueueWriteBuffer<Vertex>(_renderer.RenderingDevice.Queue, _vertexBuffer, 0, [
            new(new(-0.5f, -0.5f, 0), Vector2.Zero),
            new(new(0.5f, -0.5f, 0), Vector2.UnitX),
            new(new(0.5f, 0.5f, 0), Vector2.One),
            new(new(-0.5f, 0.5f, 0), Vector2.UnitY),
        ], (nuint)sizeof(Vertex) * 4);
        
        _renderer.WebGPU.QueueWriteBuffer<ushort>(_renderer.RenderingDevice.Queue, _indexBuffer, 0, [
            0, 1, 2, 2, 3, 0,
        ], (nuint)sizeof(ushort) * 6);
        
        // Create material.
        using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Shader.wgsl"), FileMode.Open, FileAccess.Read, FileShare.Read)) {
            byte[] buffer = new byte[fs.Length + 1];
            fs.ReadExactly(buffer.AsSpan(0, (int)fs.Length));

            fixed (byte* ptr = buffer) {
                ShaderModuleWGSLDescriptor wgslDescriptor = new() {
                    Chain = new() {
                        SType = SType.ShaderModuleWgslDescriptor,
                    },

                    Code = ptr,
                };

                _shaderModule = _renderer.WebGPU.DeviceCreateShaderModule(_renderer.RenderingDevice.Device, new ShaderModuleDescriptor {
                    NextInChain = &wgslDescriptor.Chain,
                });
            }
        }

        BindGroupLayoutEntry* bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[] {
        };

        _bindGroupLayout = _renderer.WebGPU.DeviceCreateBindGroupLayout(_renderer.RenderingDevice.Device, new BindGroupLayoutDescriptor {
            Entries = bindGroupLayoutEntries,
            EntryCount = 0,
        });

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[1] {
            _bindGroupLayout,
        };

        _pipelineLayout = _renderer.WebGPU.DeviceCreatePipelineLayout(_renderer.RenderingDevice.Device,new PipelineLayoutDescriptor {
            BindGroupLayouts = bindGroupLayouts,
            BindGroupLayoutCount = 1,
        });

        fixed (byte* vsEntrypoint = "vsmain\0"u8, psEntrypoint = "psmain\0"u8) {
            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[2] {
                new VertexAttribute {
                    Format = VertexFormat.Float32x3,
                    Offset = 0,
                    ShaderLocation = 0,
                },
                new VertexAttribute {
                    Format = VertexFormat.Float32x2,
                    Offset = (ulong)sizeof(Vector3),
                    ShaderLocation = 1,
                },
            };
            
            VertexBufferLayout* vertexBufferLayouts = stackalloc VertexBufferLayout[1] {
                new VertexBufferLayout {
                    ArrayStride = (ulong)sizeof(Vertex),
                    Attributes = vertexAttributes,
                    AttributeCount = 2,
                    StepMode = VertexStepMode.Vertex,
                },
            };

            BlendState blend = new() {
                Color = new() {
                    SrcFactor = BlendFactor.One,
                    Operation = BlendOperation.Add,
                    DstFactor = BlendFactor.Zero,
                },
                Alpha = new() {
                    SrcFactor = BlendFactor.One,
                    Operation = BlendOperation.Add,
                    DstFactor = BlendFactor.Zero,
                },
            };

            ColorTargetState* targets = stackalloc ColorTargetState[1] {
                new ColorTargetState {
                    Format = TextureFormat.Bgra8Unorm,
                    WriteMask = ColorWriteMask.All,
                    Blend = &blend,
                },
            };

            FragmentState fragmentState = new() {
                EntryPoint = psEntrypoint,
                Module = _shaderModule,
                Targets = targets,
                TargetCount = 1,
            };
            
            _renderPipeline = _renderer.WebGPU.DeviceCreateRenderPipeline(_renderer.RenderingDevice.Device, new RenderPipelineDescriptor {
                DepthStencil = null,
                Vertex = new() {
                    EntryPoint = vsEntrypoint,
                    Module = _shaderModule,
                    Buffers = vertexBufferLayouts,
                    BufferCount = 1,
                },
                Fragment = &fragmentState,
                Multisample = new() {
                    AlphaToCoverageEnabled = false,
                    Count = 1,
                    Mask = 0xFFFFFFFF,
                },
                Primitive = new() {
                    CullMode = CullMode.Back,
                    FrontFace = FrontFace.Ccw,
                    StripIndexFormat = IndexFormat.Undefined,
                    Topology = PrimitiveTopology.TriangleList,
                },
                Layout = _pipelineLayout,
            });
        }
        
        // Drawing
        BindGroupEntry* bindGroupEntries = stackalloc BindGroupEntry[] {
        };
        
        _bindGroup = _renderer.WebGPU.DeviceCreateBindGroup(_renderer.RenderingDevice.Device, new BindGroupDescriptor {
            Entries = bindGroupEntries,
            EntryCount = 0,
            Layout = _bindGroupLayout,
        });
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
        
        webgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, _vertexBuffer, 0, (ulong)sizeof(Vertex) * 4);
        webgpu.RenderPassEncoderSetIndexBuffer(renderPass, _indexBuffer, IndexFormat.Uint16, 0, sizeof(ushort) * 6);
        webgpu.RenderPassEncoderSetBindGroup(renderPass, 0, _bindGroup, 0, null);
        webgpu.RenderPassEncoderSetPipeline(renderPass, _renderPipeline);
        webgpu.RenderPassEncoderDrawIndexed(renderPass, 6, 1, 0, 0, 0);
        
        webgpu.RenderPassEncoderEnd(renderPass);
        
        var cmdBuffer = webgpu.CommandEncoderFinish(cmdEncoder, new CommandBufferDescriptor());
        
        webgpu.QueueSubmit(_renderer.RenderingDevice.Queue, 1, &cmdBuffer);
        
        webgpu.CommandBufferRelease(cmdBuffer);
        webgpu.CommandEncoderRelease(cmdEncoder);
        
        _renderer.Present();
    }

    public static void Shutdown() {
        if (_bindGroup != null) {
            _renderer.WebGPU.BindGroupRelease(_bindGroup);
            _bindGroup = null;
        }
        
        if (_renderPipeline != null) {
            _renderer.WebGPU.RenderPipelineRelease(_renderPipeline);
            _renderPipeline = null;
        }

        if (_pipelineLayout != null) {
            _renderer.WebGPU.PipelineLayoutRelease(_pipelineLayout);
            _pipelineLayout = null;
        }
        
        if (_bindGroupLayout != null) {
            _renderer.WebGPU.BindGroupLayoutRelease(_bindGroupLayout);
            _bindGroupLayout = null;
        }
        
        if (_shaderModule != null) {
            _renderer.WebGPU.ShaderModuleRelease(_shaderModule);
            _shaderModule = null;
        }
        
        if (_indexBuffer != null) {
            _renderer.WebGPU.BufferRelease(_indexBuffer);
            _indexBuffer = null;
        }
        
        if (_vertexBuffer != null) {
            _renderer.WebGPU.BufferRelease(_vertexBuffer);
            _vertexBuffer = null;
        }
        
        _renderer.Dispose();
        _renderer = null!;
    }
}
using Caxivitual.Lunacub;
using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;
using System.Diagnostics;
using System.Numerics;
using Buffer = Silk.NET.WebGPU.Buffer;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Lunacub.Playground;

public static unsafe class ApplicationLifecycle {
    private static RenderingSystem _renderingSystem = null!;
    private static ShaderingSystem _shaderingSystem = null!;
    private static AssimpSystem _assimpSystem = null!;
    
    private static ResourceHandle<Shader> _shader;
    private static ResourceHandle<Texture2D> _texture;
    private static ResourceHandle<Sampler> _sampler;
    private static ResourceHandle<Mesh> _mesh;

    private static Buffer* pTransformation;
    
    private static BindGroup* pBindGroup;
    private static BindGroupLayout* pBindGroupLayout;
    private static PipelineLayout* pPipelineLayout;
    private static RenderPipeline* pPipeline;
    
    public static unsafe void Initialize() {
        try {
            Application.Logger.LogInformation("Initializing application...");

            using (Application.Logger.BeginScope("[System]")) {
                _shaderingSystem = new();
                _renderingSystem = new(Application.MainWindow);
                _assimpSystem = new();
            }

            Application.Logger.LogInformation("Initializing resources...");

            using (Application.Logger.BeginScope("[Resources]")) {
                Resources.Initialize(_shaderingSystem);

                Resources.ImportEnvironment!.Deserializers.Add(nameof(Texture2DDeserializer), new Texture2DDeserializer(_renderingSystem));
                Resources.ImportEnvironment.Deserializers.Add(nameof(ShaderDeserializer), new ShaderDeserializer(_renderingSystem));
                Resources.ImportEnvironment.Deserializers.Add(nameof(SamplerDeserializer), new SamplerDeserializer(_renderingSystem));
                Resources.ImportEnvironment.Deserializers.Add(nameof(MeshDeserializer), new MeshDeserializer(_assimpSystem, _renderingSystem));
            }
            
            Application.Logger.LogInformation("Initializing rendering objects...");

            using (Application.Logger.BeginScope("[Rendering]")) {
                Application.Logger.LogInformation("Initializing resources...");
                _shader = Resources.Import<Shader>(ResourceID.Parse("d07d31086d805186899523663761f74f"));
                _sampler = Resources.Import<Sampler>(ResourceID.Parse("0195d7cfdb687a7593979168e2e62a7c"));
                _texture = Resources.Import<Texture2D>(ResourceID.Parse("7ae127d7df4853bc8e13f5b18cd893aa"));
                _mesh = Resources.Import<Mesh>(ResourceID.Parse("febcd85870715ddea807221fb5b71dc8"));

                pTransformation = _renderingSystem.WebGPU.DeviceCreateBuffer(_renderingSystem.RenderingDevice.Device, new BufferDescriptor {
                    Size = 64,
                    Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
                });
                
                using (Application.Logger.BeginScope("[Pipeline]")) {
                    Application.Logger.LogInformation("Initializing pipeline...");
                    BindGroupLayoutEntry* bglEntries = stackalloc BindGroupLayoutEntry[] {
                        new() {
                            Binding = 0,
                            Buffer = new() {
                                Type = BufferBindingType.Uniform,
                                HasDynamicOffset = false,
                                MinBindingSize = 64,
                            },
                            Visibility = ShaderStage.Vertex,
                        },
                        new() {
                            Binding = 1,
                            Texture = new() {
                                Multisampled = false,
                                SampleType = TextureSampleType.Float,
                                ViewDimension = TextureViewDimension.Dimension2D,
                            },
                            Visibility = ShaderStage.Fragment,
                        },
                        new() {
                            Binding = 2,
                            Sampler = new() {
                                Type = SamplerBindingType.Filtering,
                            },
                            Visibility = ShaderStage.Fragment,
                        },
                    };

                    pBindGroupLayout = _renderingSystem.WebGPU.DeviceCreateBindGroupLayout(_renderingSystem.RenderingDevice.Device, new BindGroupLayoutDescriptor {
                        Entries = bglEntries,
                        EntryCount = 3,
                    });

                    BindGroupLayout** bgls = stackalloc BindGroupLayout*[] {
                        pBindGroupLayout,
                    };

                    pPipelineLayout = _renderingSystem.WebGPU.DeviceCreatePipelineLayout(_renderingSystem.RenderingDevice.Device, new PipelineLayoutDescriptor {
                        BindGroupLayouts = bgls,
                        BindGroupLayoutCount = 1,
                    });

                    VertexAttribute* attributes = stackalloc VertexAttribute[] {
                        new() {
                            Format = VertexFormat.Float32x3,
                            Offset = 0,
                            ShaderLocation = 0,
                        },
                        new() {
                            Format = VertexFormat.Unorm8x4,
                            Offset = 12,
                            ShaderLocation = 1,
                        },
                        new() {
                            Format = VertexFormat.Float32x2,
                            Offset = 16,
                            ShaderLocation = 2,
                        },
                    };

                    VertexBufferLayout* layouts = stackalloc VertexBufferLayout[] {
                        new() {
                            Attributes = attributes,
                            AttributeCount = 3,
                            ArrayStride = (ulong)sizeof(Mesh.Vertex),
                            StepMode = VertexStepMode.Vertex,
                        },
                    };

                    fixed (byte* vsEntrypoint = "vsmain\0"u8, psEntrypoint = "psmain\0"u8) {
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

                        ColorTargetState target = new() {
                            Blend = &blend,
                            Format = DisplaySurface.SurfaceFormat,
                            WriteMask = ColorWriteMask.All,
                        };

                        FragmentState fragment = new() {
                            Targets = &target,
                            TargetCount = 1,
                            Constants = null,
                            ConstantCount = 0,
                            Module = _shader.Value!.Module,
                            EntryPoint = psEntrypoint,
                        };

                        RenderPipelineDescriptor descriptor = new RenderPipelineDescriptor {
                            Vertex = new() {
                                Buffers = layouts,
                                BufferCount = 1,
                                Constants = null,
                                ConstantCount = 0,
                                Module = _shader.Value!.Module,
                                EntryPoint = vsEntrypoint,
                            },
                            Fragment = &fragment,
                            DepthStencil = null,
                            Multisample = new() {
                                AlphaToCoverageEnabled = false,
                                Count = 1,
                                Mask = 0xFFFFFFFF,
                            },
                            Primitive = new() {
                                CullMode = CullMode.Back,
                                FrontFace = FrontFace.CW,
                                StripIndexFormat = IndexFormat.Undefined,
                                Topology = PrimitiveTopology.TriangleList,
                            },
                            Layout = pPipelineLayout,
                        };

                        pPipeline = _renderingSystem.WebGPU.DeviceCreateRenderPipeline(_renderingSystem.RenderingDevice.Device, &descriptor);
                    }
                    
                    Application.Logger.LogInformation("Initializing bind group...");
                    Debug.Assert(pTransformation != null && _texture.Value!.View != null && _sampler.Value!.Handle != null);

                    BindGroupEntry* entries = stackalloc BindGroupEntry[] {
                        new() {
                            Binding = 0,
                            Buffer = pTransformation,
                            Offset = 0,
                            Size = 64,
                        },
                        new() {
                            Binding = 1,
                            TextureView = _texture.Value!.View,
                        },
                        new() {
                            Binding = 2,
                            Sampler = _sampler.Value!.Handle,
                        },
                    };

                    pBindGroup = _renderingSystem.WebGPU.DeviceCreateBindGroup(_renderingSystem.RenderingDevice.Device, new BindGroupDescriptor {
                        Entries = entries,
                        EntryCount = 3,
                        Layout = pBindGroupLayout,
                    });
                }
            }
        } catch {
            DisposeObjects();
            throw;
        }
    }

    private static float _time;

    public static void Update(double deltaTime) {
        _time += (float)deltaTime;

        Matrix4x4 model = Matrix4x4.CreateFromYawPitchRoll(_time * 0.97f, _time * 1.0124f, _time * 1.025f);
        Matrix4x4 view = Matrix4x4.CreateLookToLeftHanded(new(0, 0, -2), Vector3.UnitZ, Vector3.UnitY);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(70f * float.Pi / 180f, (float)Application.MainWindow.FramebufferSize.X / Application.MainWindow.FramebufferSize.Y, 0.01f, 100f);
        
        _renderingSystem.WebGPU.QueueWriteBuffer(_renderingSystem.RenderingDevice.Queue, pTransformation, 0, model * view * projection, 64);
    }

    public static unsafe void Render(double deltaTime) {
        _renderingSystem.BeginRenderingFrame();
        
        RenderPassColorAttachment colorAttachment = new() {
            View = _renderingSystem.CurrentSurfaceView,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new() { R = 0, G = 0.2, B = 0.4, A = 0 },
            ResolveTarget = null,
        };
        
        var webgpu = _renderingSystem.WebGPU;
        
        var cmdEncoder = webgpu.DeviceCreateCommandEncoder(_renderingSystem.RenderingDevice.Device, new CommandEncoderDescriptor());
        
        var renderPass = webgpu.CommandEncoderBeginRenderPass(cmdEncoder, new RenderPassDescriptor {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = null,
        });

        Mesh mesh = _mesh.Value!;
        
        webgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, mesh.VertexBuffer, 0, mesh.VertexBufferSize);
        webgpu.RenderPassEncoderSetIndexBuffer(renderPass, mesh.IndexBuffer, IndexFormat.Uint32, 0, mesh.IndexBufferSize);
        webgpu.RenderPassEncoderSetPipeline(renderPass, pPipeline);
        webgpu.RenderPassEncoderSetBindGroup(renderPass, 0, pBindGroup, 0, null);
        webgpu.RenderPassEncoderDrawIndexed(renderPass, (uint)mesh.IndexCount, 1, 0, 0, 0);
        
        webgpu.RenderPassEncoderEnd(renderPass);
        
        var cmdBuffer = webgpu.CommandEncoderFinish(cmdEncoder, new CommandBufferDescriptor());
        
        webgpu.QueueSubmit(_renderingSystem.RenderingDevice.Queue, 1, &cmdBuffer);
        
        webgpu.CommandBufferRelease(cmdBuffer);
        webgpu.CommandEncoderRelease(cmdEncoder);
        
        _renderingSystem.Present();
    }
    
    public static void Shutdown() {
        Application.Logger.LogInformation("Shutting down application...");
        
        DisposeObjects();
    }

    private static void DisposeObjects() {
        Resources.Release(_mesh);
        Resources.Release(_sampler);
        Resources.Release(_shader);
        Resources.Release(_texture);
        
        Resources.Shutdown();
        
        if (_renderingSystem != null) {
            if (pTransformation != null) {
                _renderingSystem.WebGPU.BufferRelease(pTransformation);
                pTransformation = null;
            }

            if (pBindGroup != null) {
                _renderingSystem.WebGPU.BindGroupRelease(pBindGroup);
                pBindGroup = null;
            }

            if (pBindGroupLayout != null) {
                _renderingSystem.WebGPU.BindGroupLayoutRelease(pBindGroupLayout);
                pBindGroupLayout = null;
            }

            if (pPipelineLayout != null) {
                _renderingSystem.WebGPU.PipelineLayoutRelease(pPipelineLayout);
                pPipelineLayout = null;
            }

            if (pPipeline != null) {
                _renderingSystem.WebGPU.RenderPipelineRelease(pPipeline);
                pPipeline = null;
            }
            
            _renderingSystem.Dispose();
        }

        _shaderingSystem?.Dispose();
        _assimpSystem?.Dispose();
    }
}
using Caxivitual.Lunacub.Importing;
using Microsoft.Extensions.Logging;
using Silk.NET.Input;
using Silk.NET.WebGPU;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

internal static unsafe class ApplicationLifecycle {
    private static Renderer _renderer = null!;
    private static readonly ILogger _logger = LoggerFactory.Create(builder => {
        builder.AddConsole();
    }).CreateLogger("Program");
    
    // Material
    private static Shader _shader = null!;
    
    private static BindGroupLayout* _globalBindGroupLayout = null!;
    private static BindGroupLayout* _entityBindGroupLayout = null!;
    private static PipelineLayout* _pipelineLayout = null!;
    private static RenderPipeline* _renderPipeline = null!;
    
    // Drawing
    private static WebGPUBuffer* _transformationBuffer = null!;
    private static Sampler* _sampler = null!;
    
    private static ResourceHandle<Texture2D> _blasterTexture;
    private static ResourceHandle<Sprite> _blasterSprite;
    private static ResourceHandle<Texture2D> _blasterRayTexture;
    private static ResourceHandle<Sprite> _blasterRaySprite;
    
    private static BindGroup* _globalBindGroup;
    private static BindGroup* _blasterBindGroup;
    private static BindGroup* _blasterRayBindGroup;

    // private static GasterBlaster _blaster;
    private static readonly List<Entity> _entities = [];
    private static readonly List<Entity> _appendingEntities = [];
    private static readonly List<Entity> _deletingEntities = [];

    private static uint uniformOffsetAlignment;
    
    public static void Initialize() {
        _renderer = new(Application.MainWindow);
        Resources.Initialize(_renderer, _logger);

        SupportedLimits limits;
        _renderer.WebGPU.DeviceGetLimits(_renderer.RenderingDevice.Device, &limits);

        uniformOffsetAlignment = limits.Limits.MinUniformBufferOffsetAlignment;
        
        // Drawing Resources
        _transformationBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
            Size = ((64 + uniformOffsetAlignment - 1) & ~(uniformOffsetAlignment - 1)) * 16,
            MappedAtCreation = false,
        });

        _sampler = _renderer.WebGPU.DeviceCreateSampler(_renderer.RenderingDevice.Device, new SamplerDescriptor {
            AddressModeU = AddressMode.ClampToEdge,
            AddressModeV = AddressMode.ClampToEdge,
            AddressModeW = AddressMode.ClampToEdge,
            Compare = CompareFunction.Undefined,
            LodMinClamp = 0,
            LodMaxClamp = float.MaxValue,
            MinFilter = FilterMode.Nearest,
            MagFilter = FilterMode.Nearest,
            MipmapFilter = MipmapFilterMode.Nearest,
            MaxAnisotropy = 1,
        });
        
        // Create material.
        _shader = Shader.FromFile(_renderer, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Shader.wgsl"));

        _globalBindGroupLayout = CreateBindGroupLayout([
            new() {
                Binding = 0,
                Buffer = new() {
                    Type = BufferBindingType.Uniform,
                    HasDynamicOffset = true,
                    MinBindingSize = 64,
                },
                Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            },
        ]);

        _entityBindGroupLayout = CreateBindGroupLayout([
            new() {
                Binding = 0,
                Texture = new() {
                    Multisampled = false,
                    SampleType = TextureSampleType.Float,
                    ViewDimension = TextureViewDimension.Dimension2D,
                },
                Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            },
            new() {
                Binding = 1,
                Sampler = new() {
                    Type = SamplerBindingType.Filtering,
                },
                Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            },
        ]);

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[] {
            _entityBindGroupLayout,
            _globalBindGroupLayout,
        };

        _pipelineLayout = _renderer.WebGPU.DeviceCreatePipelineLayout(_renderer.RenderingDevice.Device, new PipelineLayoutDescriptor {
            BindGroupLayouts = bindGroupLayouts,
            BindGroupLayoutCount = 2,
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
                    SrcFactor = BlendFactor.SrcAlpha,
                    Operation = BlendOperation.Add,
                    DstFactor = BlendFactor.OneMinusSrcAlpha,
                },
                Alpha = new() {
                    SrcFactor = BlendFactor.SrcAlpha,
                    Operation = BlendOperation.Add,
                    DstFactor = BlendFactor.OneMinusSrcAlpha,
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
                Module = _shader.Module,
                Targets = targets,
                TargetCount = 1,
            };
            
            _renderPipeline = _renderer.WebGPU.DeviceCreateRenderPipeline(_renderer.RenderingDevice.Device, new RenderPipelineDescriptor {
                DepthStencil = null,
                Vertex = new() {
                    EntryPoint = vsEntrypoint,
                    Module = _shader.Module,
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
                    FrontFace = FrontFace.CW,
                    StripIndexFormat = IndexFormat.Undefined,
                    Topology = PrimitiveTopology.TriangleList,
                },
                Layout = _pipelineLayout,
            });
        }

        ImportingOperation blasterTextureImport = Resources.Import(1);
        ImportingOperation blasterSpriteImport = Resources.Import(2);
        ImportingOperation blasterRayTextureImport = Resources.Import(3);
        ImportingOperation blasterRaySpriteImport = Resources.Import(4);

        Task.WaitAll(blasterTextureImport.Task, blasterSpriteImport.Task, blasterRayTextureImport.Task, blasterRaySpriteImport.Task);
        
        _blasterTexture = blasterTextureImport.Task.Result.Convert<Texture2D>();
        _blasterSprite = blasterSpriteImport.Task.Result.Convert<Sprite>();
        _blasterRayTexture = blasterRayTextureImport.Task.Result.Convert<Texture2D>();
        _blasterRaySprite = blasterRaySpriteImport.Task.Result.Convert<Sprite>();
        
        // Drawing
        _globalBindGroup = CreateBindGroup([
            new() {
                Binding = 0,
                Buffer = _transformationBuffer,
                Size = 64,
            },
        ], _globalBindGroupLayout);
        
        _blasterBindGroup = CreateBindGroup([
            new() {
                Binding = 0,
                TextureView = _blasterTexture.Value!.View,
            },
            new() {
                Binding = 1,
                Sampler = _sampler,
            },
        ], _entityBindGroupLayout);
        
        _blasterRayBindGroup = CreateBindGroup([
            new() {
                Binding = 0,
                TextureView = _blasterRayTexture.Value!.View,
            },
            new() {
                Binding = 1,
                Sampler = _sampler,
            },
        ], _entityBindGroupLayout);

        // _blaster = new(_renderer, _blasterSprite.Value!, _blasterRaySprite.Value!, _renderPipeline, _blasterBindGroup, _blasterRayBindGroup) {
        //     Position = new(0, 5),
        //     LandingPosition = Vector2.Zero,
        // };
        
        _entities.Add(new GasterBlaster(_renderer, _blasterSprite.Value!, _blasterRaySprite.Value!, _renderPipeline, _blasterBindGroup, _blasterRayBindGroup) {
            Position = new(0, 5),
            LandingPosition = Vector2.Zero,
        });
        
        // _entities.Add(new BlasterRay(_renderer, _blasterRaySprite.Value!, _renderPipeline, _blasterRayBindGroup));

        static BindGroupLayout* CreateBindGroupLayout(ReadOnlySpan<BindGroupLayoutEntry> entries) {
            fixed (BindGroupLayoutEntry* ptr = entries) {
                return _renderer.WebGPU.DeviceCreateBindGroupLayout(_renderer.RenderingDevice.Device, new BindGroupLayoutDescriptor {
                    Entries = ptr,
                    EntryCount = (uint)entries.Length,
                });
            }
        }
    }

    public static void Update(double deltaTime) {
        _entities.AddRange(_appendingEntities);
        _appendingEntities.Clear();

        foreach (var removingEntity in _deletingEntities) {
            if (_entities.Remove(removingEntity)) {
                removingEntity.Dispose();
            }
        }

        _deletingEntities.Clear();
        
        _entities.ForEach(e => e.Update(deltaTime));
    }

    public static void Render() {
        uint transformationBufferCapacity = (uint)(_renderer.WebGPU.BufferGetSize(_transformationBuffer) / uniformOffsetAlignment);

        if (_entities.Count > transformationBufferCapacity) {
            _renderer.WebGPU.BufferDestroy(_transformationBuffer);
            _renderer.WebGPU.BufferRelease(_transformationBuffer);
            _renderer.WebGPU.BindGroupRelease(_globalBindGroup);
            
            _transformationBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
                Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
                Size = ((64 + uniformOffsetAlignment - 1) & ~(uniformOffsetAlignment - 1)) * transformationBufferCapacity * 2,
                MappedAtCreation = false,
            });
            
            _globalBindGroup = CreateBindGroup([
                new() {
                    Binding = 0,
                    Buffer = _transformationBuffer,
                    Size = 64,
                },
            ], _globalBindGroupLayout);
        }
        
        Matrix4x4 viewProjection = Matrix4x4.CreateLookToLeftHanded(new(0, 0, -1), Vector3.UnitZ, Vector3.UnitY) *
                                   Matrix4x4.CreateOrthographicLeftHanded(8f * Application.MainWindow.FramebufferSize.X / Application.MainWindow.FramebufferSize.Y, 8, 0.01f, 10f);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(_entities.Count * (int)uniformOffsetAlignment);

        try {
            for (int i = 0; i < _entities.Count; i++) {
                Matrix4x4 mvp = new Matrix4x4(_entities[i].TransformMatrix) * viewProjection;
                Unsafe.WriteUnaligned(ref buffer[i * uniformOffsetAlignment], mvp);
            }

            fixed (byte* ptr = buffer) {
                _renderer.WebGPU.QueueWriteBuffer(_renderer.RenderingDevice.Queue, _transformationBuffer, 0, ptr, (nuint)_entities.Count * uniformOffsetAlignment);
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        
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
        
        var renderPass = webgpu.CommandEncoderBeginRenderPass(cmdEncoder, new RenderPassDescriptor {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = null,
        });

        uint* dynamicOffsets = stackalloc uint[1];

        for (int i = 0; i < _entities.Count; i++) {
            var entity = _entities[i];
            
            if (entity.RenderingMesh is not { } renderingMesh) continue;
        
            RenderPipeline* pipeline = entity.RenderPipeline;
            if (pipeline == null) continue;
        
            BindGroup* bindGroup = entity.RenderBindGroup;
            if (bindGroup == null) continue;
            
            webgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, renderingMesh.VertexBuffer, 0, renderingMesh.VertexBufferSize);
            webgpu.RenderPassEncoderSetIndexBuffer(renderPass, renderingMesh.IndexBuffer, renderingMesh.IndexFormat, 0, renderingMesh.IndexBufferSize);
            webgpu.RenderPassEncoderSetBindGroup(renderPass, 0, bindGroup, 0, null);
            dynamicOffsets[0] = (uint)i * uniformOffsetAlignment;
            webgpu.RenderPassEncoderSetBindGroup(renderPass, 1, _globalBindGroup, 1, dynamicOffsets);
            webgpu.RenderPassEncoderSetPipeline(renderPass, pipeline);
            webgpu.RenderPassEncoderDrawIndexed(renderPass, renderingMesh.IndexCount, 1, 0, 0, 0);
        }
        
        webgpu.RenderPassEncoderEnd(renderPass);
        
        var cmdBuffer = webgpu.CommandEncoderFinish(cmdEncoder, new CommandBufferDescriptor());
        
        webgpu.QueueSubmit(_renderer.RenderingDevice.Queue, 1, &cmdBuffer);
        
        webgpu.CommandBufferRelease(cmdBuffer);
        webgpu.CommandEncoderRelease(cmdEncoder);
        
        _renderer.Present();
    }

    public static void Shutdown() {
        _renderer.WebGPU.BindGroupRelease(_blasterRayBindGroup);
        _renderer.WebGPU.BindGroupRelease(_blasterBindGroup);
        _renderer.WebGPU.BindGroupRelease(_globalBindGroup);
        
        var releaseStatus = Resources.Release(_blasterTexture);
        Debug.Assert(releaseStatus == ReleaseStatus.Success, $"Failed to release texture: {releaseStatus}.");
        
        releaseStatus = Resources.Release(_blasterSprite);
        Debug.Assert(releaseStatus is ReleaseStatus.Success or ReleaseStatus.NotDisposed, $"Failed to release sprite: {releaseStatus}.");
        
        releaseStatus = Resources.Release(_blasterRayTexture);
        Debug.Assert(releaseStatus == ReleaseStatus.Success, $"Failed to release texture: {releaseStatus}.");
        
        releaseStatus = Resources.Release(_blasterRaySprite);
        Debug.Assert(releaseStatus is ReleaseStatus.Success or ReleaseStatus.NotDisposed, $"Failed to release sprite: {releaseStatus}.");
        
        _entities.ForEach(e => e.Dispose());
        
        if (_renderPipeline != null) {
            _renderer.WebGPU.RenderPipelineRelease(_renderPipeline);
            _renderPipeline = null;
        }

        if (_pipelineLayout != null) {
            _renderer.WebGPU.PipelineLayoutRelease(_pipelineLayout);
            _pipelineLayout = null;
        }
        
        if (_globalBindGroupLayout != null) {
            _renderer.WebGPU.BindGroupLayoutRelease(_globalBindGroupLayout);
            _globalBindGroupLayout = null;
        }
        
        if (_entityBindGroupLayout != null) {
            _renderer.WebGPU.BindGroupLayoutRelease(_entityBindGroupLayout);
            _entityBindGroupLayout = null;
        }
        
        _shader.Dispose();

        if (_sampler != null) {
            _renderer.WebGPU.SamplerRelease(_sampler);
            _sampler = null;
        }
        
        if (_transformationBuffer != null) {
            _renderer.WebGPU.BufferRelease(_transformationBuffer);
            _transformationBuffer = null;
        }
        
        _renderer.Dispose();
        _renderer = null!;
    }
    
    public static void AddEntity(Entity entity) => _appendingEntities.Add(entity);
    public static void DeleteEntity(Entity entity) => _deletingEntities.Add(entity);
    
    private static BindGroup* CreateBindGroup(ReadOnlySpan<BindGroupEntry> entries, BindGroupLayout* layout) {
        fixed (BindGroupEntry* ptr = entries) {
            return _renderer.WebGPU.DeviceCreateBindGroup(_renderer.RenderingDevice.Device, new BindGroupDescriptor {
                Entries = ptr,
                EntryCount = (uint)entries.Length,
                Layout = layout,
            });
        }
    }
}
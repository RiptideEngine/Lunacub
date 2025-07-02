using Silk.NET.WebGPU;
using System.Numerics;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

// I love Undertale.
public sealed unsafe class GasterBlaster : Entity {
    public Sprite BlasterSprite { get; private set; }
    public Sprite RaySprite { get; private set; }

    private readonly Renderer _renderer;

    private Mesh _blasterMesh;

    private RenderPipeline* _pipeline;
    private BindGroup* _blasterBindGroup;
    private BindGroup* _rayBindGroup;

    private double _stateTimer;
    private double _animationTimer;

    private int _animationState;
    private State _state = 0;
    
    public Vector2 LandingPosition { get; set; }

    private Vector2 _startingPosition;

    private BlasterRay? _ray;

    private Vector2 _rayDisplacement;

    public GasterBlaster(Renderer renderer, Sprite blasterSprite, Sprite raySprite, RenderPipeline* pipeline, BindGroup* blasterBindGroup, BindGroup* rayBindGroup) {
        BlasterSprite = blasterSprite;
        RaySprite = raySprite;

        _renderer = renderer;

        _blasterMesh = new(_renderer);
        _blasterMesh.AllocateVertexBuffer(4);
        SetSubspriteIndex(0);        
        
        _blasterMesh.AllocateIndexBuffer(6, IndexFormat.Uint16);
        _blasterMesh.WriteIndex<ushort>([ 0, 1, 2, 2, 3, 0 ], 0);

        _pipeline = pipeline;
        _blasterBindGroup = blasterBindGroup;
        _rayBindGroup = rayBindGroup;
    }
    
    public override void Update(double deltaTime) {
        _stateTimer += deltaTime;

        switch (_state) {
            case State.Initialize:
                _startingPosition = Position;
                _state = State.FlyIn;
                _stateTimer = 0;
                break;
            
            case State.FlyIn:
                Position = Vector2.Lerp(_startingPosition, LandingPosition, Remap((float)_stateTimer, 0, 0.3f, 0, 1));

                if (_stateTimer >= 0.3f) {
                    _state = State.Charging;
                    _stateTimer = 0;
                }
                break;
            
            case State.Charging:
                if (_stateTimer >= 1f) {
                    _state = State.BlastStart;
                    _stateTimer = 0;
                }
                break;
            
            case State.BlastStart:
                if (_animationTimer >= 0.07f) {
                    _animationTimer = 0;
                    
                    _animationState++;
                    SetSubspriteIndex(_animationState);

                    switch (_animationState) {
                        case 4:
                            _ray = new(_renderer, RaySprite, _pipeline, _rayBindGroup) {
                                Position = Position + new Vector2(0, -0.25f),
                            };
                            ApplicationLifecycle.AddEntity(_ray);

                            _rayDisplacement = _ray.Position - Position;
                            break;
                        
                        case 5:
                            _state = State.Blasting;
                            _stateTimer = 0;
                            break;
                    }
                }
                
                _animationTimer += deltaTime;
                break;
            
            case State.Blasting:
                if (_animationTimer >= 0.07f) {
                    _animationTimer = 0;
                    _animationState++;
                    
                    SetSubspriteIndex(_animationState % 2 == 0 ? 4 : 5);
                }

                _animationTimer += deltaTime;

                Vector2 moveDirection = Vector2.Normalize(_startingPosition - LandingPosition);
                float speed = float.Lerp(0, 1, float.Clamp((float)_stateTimer / 4, 0, 1));
                Position += moveDirection * speed;
                _ray!.Position = Position + _rayDisplacement;
                break;
        }
        
        float Remap(float value, float fromStart, float fromEnd, float toStart, float toEnd) {
            return toStart + (value - fromStart) * (toEnd - toStart) / (fromEnd - fromStart);
        }
    }
    
    // ReSharper disable ConvertToAutoPropertyWhenPossible
    public override Mesh RenderingMesh => _blasterMesh;
    public override RenderPipeline* RenderPipeline => _pipeline;
    public override BindGroup* RenderBindGroup => _blasterBindGroup;
    // ReSharper restore ConvertToAutoPropertyWhenPossible

    public void Render(RenderPassEncoder* pEncoder) {
        var webgpu = _renderer.WebGPU;
        
        webgpu.RenderPassEncoderSetVertexBuffer(pEncoder, 0, _blasterMesh.VertexBuffer, 0, (ulong)sizeof(Vertex) * 4);
        webgpu.RenderPassEncoderSetIndexBuffer(pEncoder, _blasterMesh.IndexBuffer, IndexFormat.Uint16, 0, sizeof(ushort) * 6);
        webgpu.RenderPassEncoderSetBindGroup(pEncoder, 0, _blasterBindGroup, 0, null);
        webgpu.RenderPassEncoderSetPipeline(pEncoder, _pipeline);
        webgpu.RenderPassEncoderDrawIndexed(pEncoder, 6, 1, 0, 0, 0);
    }

    private void SetSubspriteIndex(int index) {
        if ((uint)index >= BlasterSprite.Subsprites.Count) {
            throw new ArgumentOutOfRangeException(nameof(index), "Subsprite index out of bounds.");
        }

        const float ppu = 32;
        
        Rectangle<uint> region = BlasterSprite.Subsprites[index].Region;
        Vector2D<float> textureSize = BlasterSprite.Texture.Size.As<float>();
        Rectangle<float> uv = new(region.Origin.As<float>() / textureSize, region.Size.As<float>() / textureSize);
        Vector2D<float> uvMax = uv.Max;

        var scale = Matrix3x2.CreateTranslation(-0.5f, -1f) * Matrix3x2.CreateScale(region.Size.As<float>().ToSystem() / ppu * new Vector2(1, -1));
        
        _blasterMesh.WriteVertex([
            new(new(Vector2.Transform(Vector2.Zero, scale), 0), uv.Origin.ToSystem()),
            new(new(Vector2.Transform(Vector2.UnitX, scale), 0), new(uvMax.X, uv.Origin.Y)),
            new(new(Vector2.Transform(Vector2.One, scale), 0), uvMax.ToSystem()),
            new(new(Vector2.Transform(Vector2.UnitY, scale), 0), new(uv.Origin.X, uvMax.Y)),
        ], 0);
    }

    protected override void DisposeImpl(bool disposing) {
        if (disposing) {
            _blasterMesh.Dispose();
            _ray?.Dispose();
        }
    }

    private enum State {
        Initialize,
        FlyIn,
        Charging,
        BlastStart,
        Blasting,
    }
}
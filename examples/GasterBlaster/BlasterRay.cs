using Silk.NET.WebGPU;
using System.Numerics;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed unsafe class BlasterRay : Entity {
    public Sprite Sprite { get; private set; }
    
    private Mesh _mesh;
    
    private readonly Renderer _renderer;

    private RenderPipeline* _renderPipeline;
    private BindGroup* _bindGroup;
    
    private double _animationTimer;
    private int _animationState;

    private double _stateTimer;
    
    // ReSharper disable ConvertToAutoPropertyWhenPossible
    public override Mesh RenderingMesh => _mesh;
    public override RenderPipeline* RenderPipeline => _renderPipeline;
    public override BindGroup* RenderBindGroup => _bindGroup;
    // ReSharper restore ConvertToAutoPropertyWhenPossible

    public BlasterRay(Renderer renderer, Sprite sprite, RenderPipeline* renderPipeline, BindGroup* bindGroup) {
        _renderer = renderer;
        Sprite = sprite;

        _mesh = new(_renderer);
        _mesh.AllocateVertexBuffer(8);
        _mesh.AllocateIndexBuffer(12, IndexFormat.Uint16);

        const float ppu = 32;
        
        Vector2D<float> textureSize = Sprite.Texture.Size.As<float>();
        
        Rectangle<uint> headRegion = Sprite.Subsprites[0].Region;
        Rectangle<float> headUV = new(headRegion.Origin.As<float>() / textureSize, headRegion.Size.As<float>() / textureSize);
        Vector2D<float> headUVMax = headUV.Max;
        
        var headScale = Matrix3x2.CreateTranslation(-0.5f, -1f) * Matrix3x2.CreateScale(headRegion.Size.As<float>().ToSystem() / ppu * new Vector2(1, -1));
        
        Rectangle<uint> bodyRegion = Sprite.Subsprites[1].Region;
        Rectangle<float> bodyUV = new(bodyRegion.Origin.As<float>() / textureSize, bodyRegion.Size.As<float>() / textureSize);
        Vector2D<float> bodyUVMax = headUV.Max;

        var bodyScale = Matrix3x2.CreateTranslation(-0.5f, -1f) * Matrix3x2.CreateScale(bodyRegion.Size.As<float>().ToSystem() / ppu * new Vector2(1, -1));
        
        _mesh.WriteVertex([
            new(new(Vector2.Transform(Vector2.Zero, headScale), 0), headUV.Origin.ToSystem()),
            new(new(Vector2.Transform(Vector2.UnitX, headScale), 0), new(headUVMax.X, headUV.Origin.Y)),
            new(new(Vector2.Transform(Vector2.One, headScale), 0), headUVMax.ToSystem()),
            new(new(Vector2.Transform(Vector2.UnitY, headScale), 0), new(headUV.Origin.X, headUVMax.Y)),
            
            new(new(Vector2.Transform(Vector2.UnitY, bodyScale), 0), bodyUV.Origin.ToSystem()),
            new(new(Vector2.Transform(Vector2.One, bodyScale), 0), new(bodyUVMax.X, bodyUV.Origin.Y)),
            new(new(Vector2.Transform(new(1, 1000), bodyScale), 0), bodyUVMax.ToSystem()),
            new(new(Vector2.Transform(new(0, 1000), bodyScale), 0), new(bodyUV.Origin.X, bodyUVMax.Y)),
        ], 0);
        
        _mesh.WriteIndex<ushort>([
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
        ], 0);
        
        _renderPipeline = renderPipeline;
        _bindGroup = bindGroup;
    }

    public override void Update(double deltaTime) {
        if (_animationTimer >= 0.05f) {
            _animationState = (_animationState + 1) % 2;

            Scale = new(_animationState == 0 ? 1 : 0.825f, 1);
            _animationTimer = 0;
        }

        if (_stateTimer >= 2) {
            ApplicationLifecycle.DeleteEntity(this);
            _stateTimer = 0;
        }
        
        _animationTimer += deltaTime;
        _stateTimer += deltaTime;
    }

    protected override void DisposeImpl(bool disposing) {
        _mesh?.Dispose();
    }
}
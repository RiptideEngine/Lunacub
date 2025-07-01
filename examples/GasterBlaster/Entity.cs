using Silk.NET.WebGPU;
using System.Numerics;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public abstract unsafe class Entity : BaseDisposable {
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public Vector2 Scale { get; set; } = Vector2.One;

    public Matrix3x2 TransformMatrix => Matrix3x2.CreateScale(Scale) * Matrix3x2.CreateRotation(Rotation / 180f * float.Pi) * Matrix3x2.CreateTranslation(Position);
    
    public abstract Mesh? RenderingMesh { get; }
    public abstract RenderPipeline* RenderPipeline { get; }
    public abstract BindGroup* RenderBindGroup { get; }

    public abstract void Update(double deltaTime);
}
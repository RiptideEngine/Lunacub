using Silk.NET.WebGPU;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed unsafe class Mesh : BaseDisposable {
    private readonly Renderer _renderer;

    public WebGPUBuffer* VertexBuffer { get; private set; }
    public WebGPUBuffer* IndexBuffer { get; private set; }
    
    public IndexFormat IndexFormat { get; private set; }

    public uint VertexCount {
        get {
            if (VertexBuffer == null) return 0;
            
            return (uint)(_renderer.WebGPU.BufferGetSize(VertexBuffer) / (uint)sizeof(Vertex));
        }
    }
    
    public ulong VertexBufferSize {
        get {
            if (VertexBuffer == null) return 0;
            
            return _renderer.WebGPU.BufferGetSize(VertexBuffer);
        }
    }

    public uint IndexCount {
        get {
            if (IndexBuffer == null) return 0;
            
            Debug.Assert(IndexFormat is IndexFormat.Uint16 or IndexFormat.Uint32);
            
            return (uint)(_renderer.WebGPU.BufferGetSize(IndexBuffer) / (IndexFormat == IndexFormat.Uint16 ? 2UL : 4UL));
        }
    }
    
    public ulong IndexBufferSize {
        get {
            if (IndexBuffer == null) return 0;
            
            Debug.Assert(IndexFormat is IndexFormat.Uint16 or IndexFormat.Uint32);
            
            return _renderer.WebGPU.BufferGetSize(IndexBuffer);
        }
    }

    public Mesh(Renderer renderer) {
        _renderer = renderer;
    }

    public void AllocateVertexBuffer(int count) {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        
        DisposeVertexBuffer();

        VertexBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
            MappedAtCreation = false,
            Size = (ulong)sizeof(Vertex) * (uint)count,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
        });
    }

    public void AllocateIndexBuffer(int count, IndexFormat format) {
        if (format is not IndexFormat.Uint16 and not IndexFormat.Uint32) {
            throw new InvalidEnumArgumentException("Invalid index format.", (int)format, typeof(IndexFormat));
        }
        
        DisposeIndexBuffer();
        
        IndexFormat = format;
        IndexBuffer = _renderer.WebGPU.DeviceCreateBuffer(_renderer.RenderingDevice.Device, new BufferDescriptor {
            MappedAtCreation = false,
            Size = (ulong)count * (format == IndexFormat.Uint16 ? 2UL : 4UL),
            Usage = BufferUsage.Index | BufferUsage.CopyDst,
        });
    }

    public void WriteVertex(ReadOnlySpan<Vertex> vertices, int offset) {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        uint vcount = VertexCount;
        if (offset >= vcount) return;

        fixed (Vertex* data = vertices) {
            nuint size = uint.Min((uint)vertices.Length, (uint)(vcount - offset)) * (nuint)sizeof(Vertex);
            _renderer.WebGPU.QueueWriteBuffer(_renderer.RenderingDevice.Queue, VertexBuffer, (ulong)offset * (ulong)sizeof(Vertex), data, size);
        }
    }

    public void WriteIndex<T>(ReadOnlySpan<T> indices, int offset) where T : unmanaged, IBinaryNumber<T> {
        if (typeof(T) != typeof(ushort) && typeof(T) != typeof(uint)) {
            throw new ArgumentException("Invalid generic argument type, expected UInt16 or UInt32.");
        }
        
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        
        uint icount = IndexCount;
        if (offset >= icount) return;
        
        switch (IndexFormat) {
            case IndexFormat.Uint16:
                if (typeof(T) == typeof(uint)) {
                    throw new ArgumentException("Invalid generic argument type, expected to be UInt16 for mesh with index format of Uint16.");
                }
                break;
            
            case IndexFormat.Uint32:
                if (typeof(T) == typeof(ushort)) {
                    throw new ArgumentException("Invalid generic argument type, expected to be UInt32 for mesh with index format of Uint32.");
                }
                break;
            
            default: throw new UnreachableException();
        }
        
        fixed (T* data = indices) {
            nuint size = uint.Min((uint)indices.Length, (uint)(icount - offset)) * (nuint)sizeof(T);
            _renderer.WebGPU.QueueWriteBuffer(_renderer.RenderingDevice.Queue, IndexBuffer, (ulong)offset * (ulong)sizeof(T), data, size);
        }
    }

    private void DisposeVertexBuffer() {
        if (VertexBuffer != null) {
            _renderer.WebGPU.BufferRelease(VertexBuffer);
            VertexBuffer = null;
        }
    }

    private void DisposeIndexBuffer() {
        if (IndexBuffer != null) {
            _renderer.WebGPU.BufferRelease(IndexBuffer);
            IndexBuffer = null;
        }
    }
    
    protected override void DisposeImpl(bool disposing) {
        DisposeVertexBuffer();
        DisposeIndexBuffer();
    }
}
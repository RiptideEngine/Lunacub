using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Importing;
using Silk.NET.Assimp;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using WebGpuBuffer = Silk.NET.WebGPU.Buffer;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace Lunacub.Playground;

public sealed unsafe class Mesh : IDisposable {
    private readonly RenderingSystem _renderingSystem;
    
    public WebGpuBuffer* VertexBuffer { get; private set; }
    public WebGpuBuffer* IndexBuffer { get; private set; }
    
    private Submesh[] _submeshes;
    public ReadOnlySpan<Submesh> Submeshes => _submeshes;

    public int VertexCount => VertexBuffer == null ? 0 : (int)(_renderingSystem.WebGPU.BufferGetSize(VertexBuffer) / (uint)sizeof(Vertex));
    public int IndexCount => IndexBuffer == null ? 0 : (int)(_renderingSystem.WebGPU.BufferGetSize(IndexBuffer) / sizeof(uint));

    public ulong VertexBufferSize => VertexBuffer == null ? 0 : _renderingSystem.WebGPU.BufferGetSize(VertexBuffer);
    public ulong IndexBufferSize => IndexBuffer == null ? 0 : _renderingSystem.WebGPU.BufferGetSize(IndexBuffer);
    
    private bool _disposed;

    public Mesh(RenderingSystem renderingSystem) {
        _renderingSystem = renderingSystem;
        _submeshes = [];
    }

    public void AllocateVertexBuffer(int vertexCount) {
        ArgumentOutOfRangeException.ThrowIfNegative(vertexCount);
        
        if (VertexBuffer != null) {
            _renderingSystem.WebGPU.BufferRelease(VertexBuffer);
            VertexBuffer = null;
        }

        VertexBuffer = _renderingSystem.WebGPU.DeviceCreateBuffer(_renderingSystem.RenderingDevice.Device, new BufferDescriptor {
            Size = (uint)vertexCount * (uint)sizeof(Vertex),
            Usage = BufferUsage.Vertex | BufferUsage.CopySrc | BufferUsage.CopyDst,
        });
    }

    public void AllocateIndexBuffer(int indexCount) {
        ArgumentOutOfRangeException.ThrowIfNegative(indexCount);
        
        if (IndexBuffer != null) {
            _renderingSystem.WebGPU.BufferRelease(IndexBuffer);
            IndexBuffer = null;
        }

        IndexBuffer = _renderingSystem.WebGPU.DeviceCreateBuffer(_renderingSystem.RenderingDevice.Device, new BufferDescriptor {
            Size = (uint)indexCount * sizeof(uint),
            Usage = BufferUsage.Index | BufferUsage.CopySrc | BufferUsage.CopyDst,
        });
    }

    public void SetSubmeshes(IEnumerable<Submesh> submeshes) {
        ArgumentNullException.ThrowIfNull(submeshes);
        
        // TODO: Validate submeshes.
        _submeshes = submeshes.ToArray();
    }
    
    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (VertexBuffer != null) {
            _renderingSystem.WebGPU.BufferRelease(VertexBuffer);
            VertexBuffer = null;
        }

        if (IndexBuffer != null) {
            _renderingSystem.WebGPU.BufferRelease(IndexBuffer);
            IndexBuffer = null;
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Mesh() {
        Dispose(false);
    }
    
    public readonly record struct Vertex(Vector3 Position, uint Color, Vector2 TexCoord);
    public readonly record struct Submesh(uint VertexOffset, uint VertexCount, uint IndexOffset, uint IndexCount);
}

public sealed class MeshDTO : ContentRepresentation {
    public Stream Stream { get; }

    public MeshDTO(Stream stream) {
        Stream = stream;
    }
}

public sealed class MeshImporter : Importer<MeshDTO> {
    protected override MeshDTO Import(Stream stream, ImportingContext context) {
        return new(stream);
    }
}

public sealed class MeshSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(MeshDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(MeshDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            ((MeshDTO)SerializingObject).Stream.CopyTo(outputStream);
        }
    }
}

public sealed class MeshDeserializer : Deserializer<Mesh> {
    private readonly AssimpSystem _assimpSystem;
    private readonly RenderingSystem _renderingSystem;
    
    public MeshDeserializer(AssimpSystem assimpSystem, RenderingSystem renderingSystem) {
        _assimpSystem = assimpSystem;
        _renderingSystem = renderingSystem;
    }
    
    protected unsafe override Mesh Deserialize(Stream stream, Stream optionsStream, DeserializationContext context) {
        // TODO: FileIO* implementation
        
        Debug.Assert(stream.Length <= Array.MaxLength);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)stream.Length);

        Scene* scene;
        try {
            stream.ReadExactly(buffer.AsSpan(0, (int)stream.Length));

            fixed (byte* file = buffer) {
                scene = _assimpSystem.Assimp.ImportFileFromMemory(file, (uint)stream.Length, (uint)(PostProcessPreset.ConvertToLeftHanded | PostProcessPreset.TargetRealTimeFast), (byte*)null);
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        try {
            uint vertexCount = 0, indexCount = 0;
            uint maxVertexCount = 0, maxIndexCount = 0;
            for (uint i = 0; i < scene->MNumMeshes; i++) {
                if (!((PrimitiveType)scene->MMeshes[i]->MPrimitiveTypes).HasFlag(PrimitiveType.Triangle)) continue;
                
                vertexCount += scene->MMeshes[i]->MNumVertices;
                indexCount += scene->MMeshes[i]->MNumFaces * 3;
                maxVertexCount = Math.Max(maxVertexCount, scene->MMeshes[i]->MNumVertices);
                maxIndexCount = Math.Max(maxIndexCount, scene->MMeshes[i]->MNumFaces * 3);
            }

            Mesh mesh = new Mesh(_renderingSystem);
            try {
                mesh.AllocateVertexBuffer((int)vertexCount);
                mesh.AllocateIndexBuffer((int)indexCount);

                WriteMeshGeometryData(mesh, scene, (int)maxVertexCount, (int)maxIndexCount);
                
                return mesh;
            } catch {
                mesh.Dispose();
                throw;
            }
        } finally {
            _assimpSystem.Assimp.ReleaseImport(scene);
        }
    }

    private unsafe void WriteMeshGeometryData(Mesh mesh, Scene* scene, int maxSubmeshVertexCount, int maxSubmeshIndexCount) {
        Mesh.Vertex[] vertexBuffer = ArrayPool<Mesh.Vertex>.Shared.Rent(maxSubmeshVertexCount);
        uint[] indexBuffer = ArrayPool<uint>.Shared.Rent(maxSubmeshIndexCount);

        try {
            Queue* wgpuQueue = _renderingSystem.RenderingDevice.Queue;

            fixed (Mesh.Vertex* pVertices = vertexBuffer) {
                fixed (uint* pIndices = indexBuffer) {
                    uint vertexOffset = 0, indexOffset = 0;
                    List<Mesh.Submesh> submeshes = new List<Mesh.Submesh>();
                    
                    for (uint i = 0; i < scene->MNumMeshes; i++) {
                        if (!((PrimitiveType)scene->MMeshes[i]->MPrimitiveTypes).HasFlag(PrimitiveType.Triangle)) continue;

                        AssimpMesh* assimpMesh = scene->MMeshes[i];
                        ConvertMesh(vertexBuffer, indexBuffer, assimpMesh);

                        _renderingSystem.WebGPU.QueueWriteBuffer(wgpuQueue, mesh.VertexBuffer, vertexOffset * (ulong)sizeof(Mesh.Vertex), pVertices,
                            assimpMesh->MNumVertices * (nuint)sizeof(Mesh.Vertex));
                        _renderingSystem.WebGPU.QueueWriteBuffer(wgpuQueue, mesh.IndexBuffer, indexOffset * (ulong)sizeof(Mesh.Vertex), pIndices,
                            assimpMesh->MNumFaces * 3 * sizeof(uint));

                        submeshes.Add(new(vertexOffset, assimpMesh->MNumVertices, indexOffset, assimpMesh->MNumFaces * 3));
                        
                        vertexOffset += assimpMesh->MNumVertices;
                        indexOffset += assimpMesh->MNumFaces * 3;
                    }
                    
                    mesh.SetSubmeshes(submeshes);
                }
            }
        } finally {
            ArrayPool<Mesh.Vertex>.Shared.Return(vertexBuffer);
            ArrayPool<uint>.Shared.Return(indexBuffer);
        }
    }

    private static unsafe void ConvertMesh(Span<Mesh.Vertex> vertices, Span<uint> indices, AssimpMesh* assimpMesh) {
        for (uint v = 0; v < assimpMesh->MNumVertices; v++) {
            uint color;

            if (assimpMesh->MColors[0] != null) {
                Vector4 unnormalizedColor = assimpMesh->MColors[0][v] * 255.9f;
                color = (uint)unnormalizedColor.X | ((uint)unnormalizedColor.Y << 8) | ((uint)unnormalizedColor.Z << 16) | ((uint)unnormalizedColor.W << 24);
            } else {
                color = 0xFFFFFFFF;
            }

            Vector3 uv = assimpMesh->MTextureCoords[0][v];
            
            vertices[(int)v] = new(assimpMesh->MVertices[v], color, new(uv.X, uv.Y));
        }

        fixed (uint* pIndices = indices) {
            for (uint f = 0; f < assimpMesh->MNumFaces; f++) {
                Unsafe.CopyBlockUnaligned(pIndices + f * 3, (assimpMesh->MFaces + f)->MIndices, sizeof(uint) * 3);
            }
        }
    }
}
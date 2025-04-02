using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Importing;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.WebGPU;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Lunacub.Playground;

public sealed unsafe class Shader : IDisposable {
    public ImmutableArray<uint> Source { get; private set; }
    public ShaderModule* Module { get; private set; }

    private readonly WebGPU _webgpu;
    
    internal Shader(RenderingSystem renderingSystem, ImmutableArray<uint> source, ShaderModule* module) {
        Source = source;
        Module = module;
        _webgpu = renderingSystem.WebGPU;
    }

    private void Dispose(bool disposing) {
        if (Module == null) return;
        
        _webgpu.ShaderModuleRelease(Module);
        Module = null;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Shader() {
        Dispose(false);
    }
}

public sealed unsafe class ShaderDTO : ContentRepresentation {
    public IDxcBlob* CompiledSpirv { get; }

    public ShaderDTO(IDxcBlob* spirv) {
        CompiledSpirv = spirv;
    }

    protected override void DisposeImpl(bool disposing) {
        CompiledSpirv->Release();
    }
}

public sealed class ShaderImporter : Importer<ShaderDTO> {
    private readonly ShaderingSystem _shaderingSystem;
    
    public ShaderImporter(ShaderingSystem shaderingSystem) {
        _shaderingSystem = shaderingSystem;
    }
    
    protected override unsafe ShaderDTO Import(Stream stream, ImportingContext context) {
        using ComPtr<IDxcCompiler3> compiler = _shaderingSystem.CreateCompiler();

        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)stream.Length);
        try {
            stream.ReadExactly(buffer.AsSpan(0, (int)stream.Length));

            fixed (byte* pBuffer = buffer) {
                using ComPtr<IDxcResult> result = default;

                using ComPtr<IDxcCompilerArgs> args = default;
                HResult hr = _shaderingSystem.DxcUtils->BuildArguments((char*)null, (char*)null, "lib_6_6", (char**)null, 0, null, 0, args.GetAddressOf());
                SilkMarshal.ThrowHResult(hr);

                fixed (char* pSpirv = "-spirv") {
                    args.AddArguments(&pSpirv, 1);
                }

                hr = compiler.Compile(new Buffer {
                    Encoding = 0,
                    Ptr = pBuffer,
                    Size = (nuint)buffer.Length,
                }, args.GetArguments(), args.GetCount(), null, SilkMarshal.GuidPtrOf<IDxcResult>(), (void**)result.GetAddressOf());
                SilkMarshal.ThrowHResult(hr);

                HResult status;
                result.GetStatus((int*)&status);

                if (status.IsError) {
                    using ComPtr<IDxcBlobEncoding> errorBlob = default;
                    hr = result.GetErrorBuffer(errorBlob.GetAddressOf());
                    Debug.Assert(hr.IsSuccess);

                    Bool32 known;
                    uint codePage;
                    hr = errorBlob.GetEncoding((int*)&known, &codePage);
                    Debug.Assert(hr.IsSuccess && known);
                    
                    throw new($"Failed to compile shader ({Encoding.GetEncoding((int)codePage).GetString((byte*)errorBlob.GetBufferPointer(), (int)errorBlob.GetBufferSize())}).");
                }
                
                using ComPtr<IDxcBlob> blob = default;
                hr = result.GetOutput(OutKind.Object, SilkMarshal.GuidPtrOf<IDxcBlob>(), (void**)blob.GetAddressOf(), null);
                Debug.Assert(hr.IsSuccess);

                return new(blob.Detach());
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

public sealed class ShaderSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ShaderDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ShaderDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override unsafe void SerializeObject(Stream outputStream) {
            IDxcBlob* blob = ((ShaderDTO)SerializingObject).CompiledSpirv;
            outputStream.Write(new((byte*)blob->GetBufferPointer(), (int)blob->GetBufferSize()));
        }
    }
}

public sealed class ShaderDeserializer(RenderingSystem renderingSystem) : Deserializer<Shader> {
    protected unsafe override Shader Deserialize(Stream stream, Stream optionsStream, DeserializationContext context) {
        Debug.Assert(stream.Length % 4 == 0);
        
        uint[] buffer = new uint[stream.Length / sizeof(uint)];
        stream.ReadExactly(MemoryMarshal.AsBytes<uint>(buffer));

        fixed (uint* spirv = buffer) {
            ShaderModuleSPIRVDescriptor spirvDesc = new() {
                Chain = new() {
                    SType = SType.ShaderModuleSpirvDescriptor,
                },

                Code = spirv,
                CodeSize = (uint)buffer.Length,
            };

            ShaderModule* module = renderingSystem.WebGPU.DeviceCreateShaderModule(renderingSystem.RenderingDevice.Device, new ShaderModuleDescriptor {
                Hints = null,
                HintCount = 0,
                NextInChain = &spirvDesc.Chain,
            });

            return new(renderingSystem, ImmutableCollectionsMarshal.AsImmutableArray(buffer), module);
        }
    }
}
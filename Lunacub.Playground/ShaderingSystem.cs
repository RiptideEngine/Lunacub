using Microsoft.Extensions.Logging;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lunacub.Playground;

public sealed unsafe class ShaderingSystem : IDisposable {
    private bool _disposed;
    
    public DXC Dxc { get; private set; }
    
    public IDxcUtils* DxcUtils { get; private set; }
    
    public static string? DxcLibraryDirectory {
        get {
            return RuntimeInformation.ProcessArchitecture switch {
                Architecture.Arm64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native-libraries", "dxc", "arm64"),
                Architecture.X86 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native-libraries", "dxc", "x86"),
                Architecture.X64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native-libraries", "dxc", "x64"),
                _ => null,
            };
        }
    }

    public ShaderingSystem() {
        Application.Logger.LogInformation("Initializing {name}...", GetType().Name);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            throw new NotImplementedException("DXC library for Linux is not yet installed and configurate (Lazy).");
        }
        
        if (DxcLibraryDirectory is not { } dxcLibraryDirectory) {
            throw new NotSupportedException($"DXC doesn't support {RuntimeInformation.ProcessArchitecture} architecture.");
        }
        
        string arch = RuntimeInformation.ProcessArchitecture switch {
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            _ => throw new NotSupportedException($"DXC doesn't support {RuntimeInformation.ProcessArchitecture} architecture."),
        };
        
        Dxc = new(DXC.CreateDefaultContext([
            Path.Combine(dxcLibraryDirectory, "dxcompiler.dll"),
        ]));

        Guid CLSID_DxcUtils = new(0x6245d6af, 0x66e0, 0x48fd, 0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74, 0x8c);
        DxcUtils = Dxc.CreateInstance<IDxcUtils>(ref CLSID_DxcUtils);
    }

    public IDxcCompiler3* CreateCompiler() {
        Guid CLSID_DxcCompiler = new(0x73e22d93, 0xe6ce, 0x47f3, 0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0);

        IDxcCompiler3* pCompiler;
        Dxc.CreateInstance(ref CLSID_DxcCompiler, SilkMarshal.GuidPtrOf<IDxcCompiler3>(), (void**)&pCompiler);

        return pCompiler;
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        DxcUtils->Release();
        
        if (disposing) {
            Dxc.Dispose();
            Dxc = null!;
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ShaderingSystem() {
        Dispose(false);
    }
}
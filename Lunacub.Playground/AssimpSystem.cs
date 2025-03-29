using Silk.NET.Assimp;

namespace Lunacub.Playground;

public sealed class AssimpSystem : IDisposable {
    public Assimp Assimp { get; private set; }
    private bool _disposed;
    
    public AssimpSystem() {
        Assimp = Assimp.GetApi();
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        
        Assimp.Dispose();
        Assimp = null!;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AssimpSystem() {
        Dispose(false);
    }
}
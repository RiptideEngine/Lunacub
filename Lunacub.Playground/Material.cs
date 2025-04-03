namespace Lunacub.Playground;

public sealed class Material : IDisposable {
    public Shader Shader { get; }

    private bool _disposed;
    
    public Material(Shader shader) {
        
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Material() {
        Dispose(false);
    }
}
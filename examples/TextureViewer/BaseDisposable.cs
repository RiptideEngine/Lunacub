namespace Caxivitual.Lunacub.Examples.TextureViewer;

public abstract class BaseDisposable : IDisposable {
    private bool _disposed;

    protected abstract void DisposeImpl(bool disposing);

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        DisposeImpl(disposing);
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BaseDisposable() {
        Dispose(false);
    }
}
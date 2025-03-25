namespace Caxivitual.Lunacub.Building;

public abstract class ContentRepresentation : IDisposable {
    private bool _disposed;

    protected virtual void DisposeImpl(bool disposing) { }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        DisposeImpl(disposing);
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ContentRepresentation() {
        Dispose(false);
    }
}
namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that represents a resource that being built by <see cref="BuildEnvironment"/>. 
/// </summary>
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

    [ExcludeFromCodeCoverage]
    ~ContentRepresentation() {
        Dispose(false);
    }
}
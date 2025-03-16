namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildEnvironment : IDisposable {
    public ImporterDictionary Importers { get; } = [];
    public ProcessorDictionary Processors { get; } = [];
    public SerializerCollection Serializers { get; } = [];

    public OutputSystem Output { get; }

    private readonly ReportTracker _reportTracker;
    public ResourceRegistry Resources { get; } = new();

    private bool _disposed;

    public BuildEnvironment(OutputSystem output) {
        Output = output;
        _reportTracker = new(output);
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            Resources.Dispose();
        }
        
        _reportTracker.FlushPendingReports();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BuildEnvironment() {
        Dispose(false);
    }
}
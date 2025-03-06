namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildingContext(BuildOutput output) : IDisposable {
    public ImporterDictionary Importers { get; } = [];
    public ProcessorDictionary Processors { get; } = [];
    public SerializerCollection Serializers { get; } = [];

    public BuildOutput Output { get; } = output;

    private readonly ReportTracker _reportTracker = new(output);
    public ResourceRegistry Resources { get; } = new();

    private bool _disposed;

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

    ~BuildingContext() {
        Dispose(false);
    }
}
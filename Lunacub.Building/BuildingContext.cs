namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildingContext : IDisposable {
    public string ReportDirectory => _reportTracker.ReportDirectory;
    public string OutputDirectory { get; }
    
    public ImporterDictionary Importers { get; }
    public ProcessorDictionary Processors { get; }
    public SerializerCollection Serializers { get; }
    
    private readonly ReportTracker _reportTracker;
    public ResourceRegistry Resources { get; }

    private bool _disposed;
    
    public BuildingContext(string reportDirectory, string outputDirectory) {
        _reportTracker = new(reportDirectory);
        
        OutputDirectory = Path.GetFullPath(outputDirectory);
        
        if (!Directory.Exists(OutputDirectory)) {
            throw new ArgumentException($"Output directory '{OutputDirectory}' does not exist.");
        }
        
        Importers = [];
        Processors = [];
        Serializers = [];

        Resources = new();
    }

    public void FlushReports() {
        _reportTracker.FlushPendingReports();
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            Resources.Dispose();
        }
        
        FlushReports();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BuildingContext() {
        Dispose(false);
    }
}
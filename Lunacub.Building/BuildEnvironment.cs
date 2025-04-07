namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildEnvironment : IDisposable {
    public ImporterDictionary Importers { get; } = [];
    public ProcessorDictionary Processors { get; } = [];
    public SerializerFactoryCollection SerializerFactories { get; } = [];
    public OutputSystem Output { get; }
    public ResourceRegistry Resources { get; } = new();
    internal IncrementalInfoStorage IncrementalInfos { get; }
    
    private bool _disposed;

    public BuildEnvironment(OutputSystem output) {
        Output = output;
        IncrementalInfos = new(output);
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            Resources.Dispose();
        }
        
        Output.FlushIncrementalInfos(IncrementalInfos);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~BuildEnvironment() {
        Dispose(false);
    }
}
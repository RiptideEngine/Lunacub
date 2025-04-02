namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildEnvironment : IDisposable {
    public ImporterDictionary Importers { get; } = [];
    public ProcessorDictionary Processors { get; } = [];
    public SerializerFactoryCollection SerializersFactory { get; } = [];
    public OutputSystem Output { get; }
    public ResourceRegistry Resources { get; } = new();

    private readonly IncrementalInfoStorage _incrementalInfoStorage;
    private bool _disposed;

    public BuildEnvironment(OutputSystem output) {
        Output = output;
        _incrementalInfoStorage = new(output);
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            Resources.Dispose();
        }
        
        Output.FlushIncrementalInfos(_incrementalInfoStorage);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BuildEnvironment() {
        Dispose(false);
    }
}
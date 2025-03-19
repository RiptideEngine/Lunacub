using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Caxivitual.Lunacub.Building;

public sealed partial class BuildEnvironment : IDisposable {
    public ImporterDictionary Importers { get; } = [];
    public ProcessorDictionary Processors { get; } = [];
    public SerializerCollection Serializers { get; } = [];
    public OutputSystem Output { get; }
    public ResourceRegistry Resources { get; } = new();

    public ILogger Logger { get; set; }

    private readonly IncrementalInfoStorage _incrementalInfoStorage;
    private bool _disposed;

    public BuildEnvironment(OutputSystem output) {
        Output = output;
        _incrementalInfoStorage = new(output);
        Logger = NullLogger.Instance;
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
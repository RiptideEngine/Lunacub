using Caxivitual.Lunacub.Importing.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Caxivitual.Lunacub.Importing;

public sealed partial class ImportEnvironment : IDisposable {
    public InputSystem Input { get; }
    public DeserializerDictionary Deserializers { get; }
    public DisposerCollection Disposers { get; }

    private ILogger _logger;
    public ILogger Logger {
        get => _logger;
        
        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        set => _logger = value ?? NullLogger.Instance;
        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    }
    
    public Statistics Statistics { get; } = new();
    
    private bool _disposed;
    
    public ImportEnvironment() {
        Input = new();
        Deserializers = [];
        Disposers = [];
        _resourceCache = new(this);
        _logger = NullLogger.Instance;
    }
    
    // public ResourceHandle Import(ResourceID rid) => Import<object>(rid);
    //
    // public ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
    //     return _resources.Import<T>(rid);
    // }
    //
    // public void ImportFromTags(string query, ICollection<ResourceHandle> outputList) {
    //     _resources.ImportFromTags(query, outputList);
    // }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            _resourceCache.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ImportEnvironment() {
        Dispose(false);
    }
}
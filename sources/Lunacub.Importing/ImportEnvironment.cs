using Caxivitual.Lunacub.Importing.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;

namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents the environment that handles the resource importing and caching processes.
/// </summary>
public sealed partial class ImportEnvironment : IDisposable {
    /// <summary>
    /// The compiled resource entrypoints.
    /// </summary>
    public ResourceLibraryCollection Libraries { get; }
    
    /// <summary>
    /// Gets the dictionary of <see cref="Deserializer"/> that compiled resources request.
    /// </summary>
    public DeserializerDictionary Deserializers { get; }
    
    /// <summary>
    /// Gets the collection of <see cref="Disposer"/> that handles resource disposal.
    /// </summary>
    public DisposerCollection Disposers { get; }

    private ILogger _logger;
    
    /// <summary>
    /// Gets the logging object that used for error reporting purpose.
    /// </summary>
    public ILogger Logger {
        get => _logger;
        
        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        set => _logger = value ?? NullLogger.Instance;
        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    }
    
    /// <summary>
    /// Gets the statistics object for debugging purpose.
    /// </summary>
    public Statistics Statistics { get; } = new();
    
    private bool _disposed;
    
    internal RecyclableMemoryStreamManager MemoryStreamManager { get; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ImportEnvironment"/> with empty components and null logger.
    /// </summary>
    /// <param name="memoryStreamManager">
    /// An instance of memory stream manager that <see cref="Deserializer"/> can use.
    /// </param>
    public ImportEnvironment(RecyclableMemoryStreamManager memoryStreamManager) {
        Libraries = [];
        Deserializers = [];
        Disposers = [];
        _importDispatcher = new(this);
        _logger = NullLogger.Instance;
        MemoryStreamManager = memoryStreamManager;
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            _importDispatcher.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~ImportEnvironment() {
        Dispose(false);
    }
}
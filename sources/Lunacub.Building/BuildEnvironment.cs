using Caxivitual.Lunacub.Building.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the environment that stores all the components needed to build resources.
/// </summary>
/// <remarks>This class is not thread-safe.</remarks>
public sealed partial class BuildEnvironment : IDisposable {
    /// <summary>
    /// Gets the dictionary of <see cref="Importer"/> that compiling resources request.
    /// </summary>
    /// <seealso cref="BuildingOptions.ImporterName"/>
    public ImporterDictionary Importers { get; } = [];
    
    /// <summary>
    /// Gets the dictionary of <see cref="Processor"/> that compiling resources request.
    /// </summary>
    /// <seealso cref="BuildingOptions.ProcessorName"/>
    public ProcessorDictionary Processors { get; } = [];
    
    /// <summary>
    /// Gets the collection of <see cref="SerializerFactory"/>.
    /// </summary>
    public SerializerFactoryCollection SerializerFactories { get; } = [];
    
    /// <summary>
    /// Gets the collection of resources need to be built.
    /// </summary>
    public ResourceDictionary Resources { get; }
    
    /// <summary>
    /// Gets the <see cref="OutputSystem"/> instance.
    /// </summary>
    public OutputSystem Output { get; }
    
    /// <summary>
    /// Gets the collection that stores the <see cref="IncrementalInfo"/>.
    /// </summary>
    internal IncrementalInfoStorage IncrementalInfos { get; }
    
    /// <summary>
    /// Gets and sets the <see cref="ILogger"/> instance used for debugging and reporting purpose.
    /// </summary>
    public ILogger Logger { get; set; }
    
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="BuildEnvironment"/> with empty <see cref="Importers"/>,
    /// <see cref="Processors"/>, <see cref="SerializerFactories"/>, <see cref="Resources"/> and has the provided
    /// <see cref="OutputSystem"/> object.
    /// </summary>
    /// <param name="output">The <see cref="OutputSystem"/> object for the instance to use.</param>
    public BuildEnvironment(OutputSystem output) {
        Output = output;
        Resources = new();
        IncrementalInfos = new(output);
        Logger = NullLogger.Instance;
    }
    
    public BuildingResult BuildResources() {
        DateTime begin = DateTime.Now;
        
        BuildSession session = new BuildSession(this);
        session.Build();

        return new(begin, DateTime.Now, session.Results);
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
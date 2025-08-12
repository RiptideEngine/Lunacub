using Caxivitual.Lunacub.Building.Collections;
using Microsoft.IO;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Represents the environment that stores all the components needed to build resources.
/// </summary>
/// <remarks>This class is not thread-safe.</remarks>
public sealed class BuildEnvironment : IDisposable {
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
    public ResourceLibraryCollection Libraries { get; }
    
    /// <summary>
    /// Gets the <see cref="OutputSystem"/> instance.
    /// </summary>
    public OutputSystem Output { get; }
    
    /// <summary>
    /// Gets the collection that stores the <see cref="IncrementalInfo"/>.
    /// </summary>
    internal EnvironmentIncrementalInfos IncrementalInfos { get; }
    
    /// <summary>
    /// Gets the procedural schematic of the resources.
    /// </summary>
    internal EnvironmentProceduralSchematic ProceduralSchematic { get; }
    
    /// <summary>
    /// Gets and sets the <see cref="ILogger"/> instance used for debugging and reporting purpose.
    /// </summary>
    public ILogger Logger { get; set; }
    
    internal RecyclableMemoryStreamManager MemoryStreamManager { get; }
    
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="BuildEnvironment"/> with empty <see cref="Importers"/>,
    /// <see cref="Processors"/>, <see cref="SerializerFactories"/>, <see cref="Libraries"/> and has the provided
    /// <see cref="OutputSystem"/> object.
    /// </summary>
    /// <param name="output">The <see cref="OutputSystem"/> object for the instance to use.</param>
    /// <param name="memoryStreamManager">
    /// An instance of memory stream manager that used to temporary stores the built resource output.
    /// </param>
    public BuildEnvironment(OutputSystem output, RecyclableMemoryStreamManager memoryStreamManager) {
        Output = output;
        Libraries = [];
        IncrementalInfos = new();
        ProceduralSchematic = new();
        Logger = NullLogger.Instance;
        MemoryStreamManager = memoryStreamManager;
        
        Output.CollectIncrementalInfos(IncrementalInfos);
        Output.CollectProceduralSchematic(ProceduralSchematic);
    }
    
    /// <summary>
    /// Build all the registered resources from registered resource libraries.
    /// </summary>
    /// <returns>An structure that contains all the building resources.</returns>
    public BuildingResult BuildResources() {
        DateTime begin = DateTime.Now;
        
        BuildSession session = new BuildSession(this);
        session.Build();

        return new(begin, DateTime.Now, session.Results);
    }

    public void FlushIncrementalInfos() {
        Output.FlushIncrementalInfos(IncrementalInfos);
    }

    public void FlushProceduralSchematic() {
        Output.FlushProceduralSchematic(ProceduralSchematic);
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            FlushIncrementalInfos();
            FlushProceduralSchematic();
        }
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
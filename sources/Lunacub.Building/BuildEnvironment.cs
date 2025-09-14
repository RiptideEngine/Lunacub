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
    /// Gets the <see cref="IResourceSink"/> instance that associates with this <see cref="BuildEnvironment"/>.
    /// </summary>
    public IResourceSink ResourceSink { get; }
    
    /// <summary>
    /// Gets the <see cref="IBuildCacheRepository"/> instance that associates with this <see cref="BuildEnvironment"/>.
    /// </summary>
    public IBuildCacheRepository BuildCacheRepository { get; }
    
    /// <summary>
    /// Gets the <see cref="IBuildCacheSink"/> instance that associates with this <see cref="BuildEnvironment"/>.
    /// </summary>
    public IBuildCacheSink BuildCacheSink { get; }
    
    /// <summary>
    /// Gets the collection that stores the <see cref="Incremental.BuildCache"/>.
    /// </summary>
    internal EnvironmentBuildCache BuildCache { get; }
    
    /// <summary>
    /// Gets and sets the <see cref="ILogger"/> instance used for debugging and reporting purpose.
    /// </summary>
    public ILogger Logger { get; set; }
    
    /// <summary>
    /// Gets the dictionary that contains dynamic environment variables.
    /// </summary>
    public Dictionary<object, object> EnvironmentVariables { get; }
    
    internal RecyclableMemoryStreamManager MemoryStreamManager { get; }
    
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="BuildEnvironment"/> with provided parameters.
    /// </summary>
    /// <param name="resourceSink">
    /// The sink endpoint that will be used to flush the compiled binary of resources and
    /// library registries.
    /// </param>
    /// <param name="buildCacheRepository">
    /// The repository of resources' incremental information.
    /// </param>
    /// <param name="buildCacheSink">
    /// The sink endpoint that will be used to flush resources' incremental information.
    /// </param>
    /// <param name="memoryStreamManager">
    /// An instance of memory stream manager that used to temporary stores the built resource output.
    /// </param>
    public BuildEnvironment(IResourceSink resourceSink, IBuildCacheRepository buildCacheRepository, IBuildCacheSink buildCacheSink, RecyclableMemoryStreamManager memoryStreamManager) {
        Libraries = [];
        BuildCache = new();
        Logger = NullLogger.Instance;
        MemoryStreamManager = memoryStreamManager;
        EnvironmentVariables = [];
        
        ResourceSink = resourceSink;
        BuildCacheRepository = buildCacheRepository;
        BuildCacheSink = buildCacheSink;
        
        BuildCacheRepository.CollectIncrementalInfos(BuildCache);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BuildEnvironment"/> with provided parameters.
    /// </summary>
    /// <param name="resourceSink">
    /// The sink endpoint that will be used to flush the compiled binary of resources and
    /// library registries.
    /// </param>
    /// <param name="buildCacheIO">
    /// An instance that handle the input/output operation of resources' build cache.
    /// </param>
    /// <param name="memoryStreamManager">
    /// An instance of memory stream manager that used to temporary stores the built resource output.
    /// </param>
    public BuildEnvironment(IResourceSink resourceSink, IBuildCacheIO buildCacheIO, RecyclableMemoryStreamManager memoryStreamManager) :
        this(resourceSink, buildCacheIO, buildCacheIO, memoryStreamManager) {
    }

    /// <summary>
    /// Build all the registered resources from registered resource libraries.
    /// </summary>
    /// <returns>An structure that contains all the building resources.</returns>
    public BuildingResult BuildResources(BuildFlags flags = BuildFlags.None) {
        DateTime begin = DateTime.Now;
        
        BuildSession session = new BuildSession(this);
        session.Build(flags.HasFlag(BuildFlags.Rebuild));

        return new(begin, DateTime.Now, session.Results);
    }

    public void FlushBuildCaches() {
        BuildCacheSink.FlushBuildCaches(BuildCache);
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            FlushBuildCaches();
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
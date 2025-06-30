// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Building.Exceptions;
using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Building;

internal sealed partial class BuildSession {
    private readonly BuildEnvironment _environment;

    public Dictionary<ResourceID, ResourceBuildingResult> Results { get; }
    private Dictionary<ResourceID, OutputRegistryElement> _outputRegistry;
    
    private readonly Dictionary<ResourceID, EnvironmentResourceVertex> _graph;
    private Dictionary<ResourceID, BuildingProceduralResource> _proceduralResources;
    
    public BuildSession(BuildEnvironment environment) {
        _environment = environment;
        _graph = new(_environment.Libraries.Sum(x => x.Registry.Count));
        Results = new();
        _outputRegistry = [];

        _proceduralResources = [];
    }

    public void Build() {
        Results.Clear();
        _outputRegistry.Clear();
        
        BuildEnvironmentResources();
        
        Debug.Assert(_graph.Values.All(x => x.ImportOutput == null || IsDisposed(x.ImportOutput)), "Resource leaked after building environment resources.");
        
        BuildProceduralResources();
        
        Debug.Assert(_graph.Values.All(x => x.ImportOutput == null || IsDisposed(x.ImportOutput)), "Resource leaked after building procedural resources.");

        _environment.Output.OutputResourceRegistry(_outputRegistry);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_disposed")]
    private static extern ref bool IsDisposed(ContentRepresentation contentRepresentation);

    private void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
        foreach (var dependencyId in dependencyIds) {
            if (!_graph.TryGetValue(dependencyId, out EnvironmentResourceVertex? resourceVertex)) continue;
            
            resourceVertex.Release();
        }
    }

    private void SerializeProcessedObject(ContentRepresentation processed, ResourceID rid, IImportOptions? options, IReadOnlyCollection<string> tags) {
        var factory = _environment.SerializerFactories.GetSerializableFactory(processed.GetType())!;
        
        if (factory == null) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializerFactory, processed.GetType()));
        }

        using MemoryStream ms = new(4096);

        var serializer = factory.InternalCreateSerializer(processed, new(options, _environment.Logger));
        
        CompileHelpers.Compile(serializer, ms, tags);
        ms.Position = 0;
        _environment.Output.CopyCompiledResourceOutput(ms, rid);
    }

    private void AppendProceduralResources(ResourceID rid, IReadOnlyDictionary<ProceduralResourceID, BuildingProceduralResource> proceduralResources, Dictionary<ResourceID, BuildingProceduralResource> receiver) {
        foreach ((var proceduralId, var proceduralResource) in proceduralResources) {
            ResourceID hashedId = rid.Combine(proceduralId);

            Debug.Assert(!_environment.Libraries.ContainsResource(hashedId), "ResourceID collided.");
            receiver.Add(hashedId, proceduralResource);
        }
    }
    
    /// <summary>
    /// Determines whether a resource should be rebuilt based on timeline, configurations, dependencies from previous build informations.
    /// </summary>
    /// <param name="rid">Resource to determines whether rebuilding needed.</param>
    /// <param name="sourceLastWriteTimes">The last write times of resource's sources.</param>
    /// <param name="currentOptions">Building options of the resource.</param>
    /// <param name="currentDependencies">Dependencies of the resource.</param>
    /// <param name="previousIncrementalInfo">
    ///     When this method returns, contains the <see cref="IncrementalInfo"/> of the previous building session of the resource. If the
    ///     resource hasn't been build before, <see langword="default"/> is returned.
    /// </param>
    /// <returns><see langword="true"/> if the resource should be rebuilt; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The function does not account for the version of building components.</remarks>
    private bool IsResourceCacheable(ResourceID rid, SourceLastWriteTimes sourceLastWriteTimes, BuildingOptions currentOptions, IReadOnlySet<ResourceID> currentDependencies, out IncrementalInfo previousIncrementalInfo) {
        // If resource has been built before, and have old report, we can begin checking for caching.
        if (_environment.Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && _environment.IncrementalInfos.TryGet(rid, out previousIncrementalInfo)) {
            if (CompareLastWriteTimes(previousIncrementalInfo.SourcesLastWriteTime, sourceLastWriteTimes)) {
                // If the options are equal, no need to rebuild.
                if (currentOptions.Equals(previousIncrementalInfo.Options)) {
                    if (previousIncrementalInfo.Dependencies.SequenceEqual(currentDependencies)) {
                        return true;
                    }
                }
            }
    
            return false;
        }
    
        previousIncrementalInfo = default;
        return false;

        // Check if resource's last write time is the same as the time stored in report.
        // Check if destination's last write time is later than resource's last write time.
        static bool CompareLastWriteTimes(SourceLastWriteTimes previous, SourceLastWriteTimes current) {
            if (previous.Primary != current.Primary) return false;
            if (previous.Secondaries == null) return current.Secondaries == null;

            if (current.Secondaries == null) return false;
                
            foreach ((var previousSourceName, var previousSourceLastWriteTime) in previous.Secondaries) {
                if (!current.Secondaries.TryGetValue(previousSourceName, out var curentSourceLastWriteTime)) return false;
                if (previousSourceLastWriteTime != curentSourceLastWriteTime) return false;
            }

            return true;
        }
    }

    private sealed class EnvironmentResourceVertex {
        public readonly BuildResourceLibrary Library;
        
        /// <summary>
        /// Gets the Id of the dependency resources, the collection is unsanitied, thus it can reference unregistered resource
        /// id, or self-referencing.
        /// </summary>
        public IReadOnlySet<ResourceID> DependencyIds;
        public readonly ResourceRegistry<BuildingResource>.Element RegistryElement;
        
        public ContentRepresentation? ImportOutput { get; private set; }
        
        public int ReferenceCount;

        public EnvironmentResourceVertex(BuildResourceLibrary library, IReadOnlySet<ResourceID> dependencies, ResourceRegistry<BuildingResource>.Element registryElement) {
            Library = library;
            DependencyIds = dependencies;
            RegistryElement = registryElement;
        }

        [MemberNotNull(nameof(ImportOutput))]
        public void SetImportResult(ContentRepresentation importOutput) {
            // Debug.Assert(ReferenceCount > 0);
            
            ImportOutput = importOutput;
        }
        
        public void Release() {
            if (--ReferenceCount != 0) return;

            ImportOutput?.Dispose();
            ImportOutput = null;
        }
    }
}
// ReSharper disable VariableHidesOuterVariable

// TODO:
// 04-08-2025: Using ProceduralSchematic, detect if the compiled resource file missing to trigger compilation of that resource.
               
using Caxivitual.Lunacub.Building.Collections;

namespace Caxivitual.Lunacub.Building;

internal sealed partial class BuildSession {
    private readonly BuildEnvironment _environment;

    public EnvironmentLibraryDictionary<ResourceResultDictionary> Results { get; }
    
    private readonly Dictionary<ResourceAddress, EnvironmentResourceVertex> _graph;
    
    private readonly Dictionary<LibraryID, ResourceRegistry<ResourceRegistry.Element>> _outputRegistries;
    private readonly EnvironmentProceduralSchematic _overrideProceduralSchematic;
    
    private readonly Dictionary<ResourceAddress, ProceduralResourceRequest> _proceduralResources;
    
    public BuildSession(BuildEnvironment environment) {
        _environment = environment;
        _graph = new(_environment.Libraries.Sum(x => x.Registry.Count));
        Results = new();
        _outputRegistries = [];
        _overrideProceduralSchematic = new();

        _proceduralResources = [];
    }

    public void Build() {
        Results.Clear();
        _outputRegistries.Clear();
        
        // This is where the fun begin.
        BuildEnvironmentResources();
        
        Debug.Assert(
            _graph.Values.All(x => x.ImportOutput == null || IsDisposed(x.ImportOutput)), 
            "Resource leaked after building environment resources."
        );
        
        BuildProceduralResources();
        
        Debug.Assert(
            _graph.Values.All(x => x.ImportOutput == null || IsDisposed(x.ImportOutput)),
            "Resource leaked after building procedural resources."
        );
        // Welp, fun is over.
        
        // Post-compilation incremental info processing and flushing.
        
        // Merge our procedural schematic to environment.
        
        GC.KeepAlive(_environment.ProceduralSchematic);
        
        // The override procedural schematic only contains successfully build resource, it doesn't contains cached resources.
        foreach ((var libraryId, var overrideLibraryProceduralSchematic) in _overrideProceduralSchematic) {
            if (_environment.ProceduralSchematic.TryGetValue(libraryId, out var envLibraryProceduralSchematic)) {
                envLibraryProceduralSchematic.Clear();
                
                foreach ((var resourceId, var overrideSchematic) in overrideLibraryProceduralSchematic) {
                    envLibraryProceduralSchematic.Add(resourceId, overrideSchematic);
                }
            } else {
                // Newly built library?
                _environment.ProceduralSchematic.Add(libraryId, overrideLibraryProceduralSchematic);
            }
        }
        
        // When the resource is cached, the registry does not contains our procedural generated resource.
        // This is where the procedural schematic came into used.
        
        GC.KeepAlive(_environment.ProceduralSchematic);
        
        foreach ((LibraryID libraryId, ResourceRegistry<ResourceRegistry.Element> registry) in _outputRegistries) {
            // How this processing works:
            // Enumerate through the resource which got cached, convert the old procedural schematic into the registry element
            // and add it to the registry.

            // If the library got procedural schematic.
            if (_environment.ProceduralSchematic.TryGetValue(libraryId, out var libraryProceduralSchematic)) {
                bool getSuccessful = Results.TryGetValue(libraryId, out var libraryResults);
                Debug.Assert(getSuccessful);

                foreach ((var resourceId, var result) in libraryResults!) {
                    if (result.Status != BuildStatus.Cached) continue;

                    getSuccessful = libraryProceduralSchematic!.TryGetValue(resourceId, out var resourceProceduralSchematic);
                    Debug.Assert(getSuccessful);

                    foreach ((var proceduralResourceId, var tags) in resourceProceduralSchematic!) {
                        registry.Add(proceduralResourceId, new(null, tags));
                    }
                }
            }

            // Output the registry.
            _environment.Output.OutputLibraryRegistry(registry, libraryId);
        }
        
        _environment.FlushProceduralSchematic();
        _environment.FlushIncrementalInfos();
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_disposed")]
    private static extern ref bool IsDisposed(ContentRepresentation contentRepresentation);

    private void ReleaseDependencies(IReadOnlyCollection<ResourceAddress> dependencyAddresses) {
        foreach (var dependencyAddress in dependencyAddresses) {
            if (!_graph.TryGetValue(dependencyAddress, out EnvironmentResourceVertex? resourceVertex)) continue;
            
            resourceVertex.Release();
        }
    }

    private void AddOutputResourceRegistry(ResourceAddress address, ResourceRegistry.Element element) {
        ref var registry = ref CollectionsMarshal.GetValueRefOrAddDefault(_outputRegistries, address.LibraryId, out bool exists);

        if (!exists) {
            registry = [];
        }
        
        registry!.Add(address.ResourceId, element);
    }

    private void AddOverrideProceduralSchematicEdge(ResourceAddress sourceResourceAddress, ProceduralResourceSchematicInfo info) {
        if (_overrideProceduralSchematic.TryGetValue(sourceResourceAddress.LibraryId, out var librarySchematic)) {
            librarySchematic.Add(sourceResourceAddress.ResourceId, info);
        } else {
            librarySchematic = [];
            librarySchematic.Add(sourceResourceAddress.ResourceId, info);
            _overrideProceduralSchematic.Add(sourceResourceAddress.LibraryId, librarySchematic);
        }
    }

    private void SetResult(ResourceAddress address, ResourceBuildingResult result) {
        if (Results.TryGetValue(address.LibraryId, out var resourceResults)) {
            resourceResults[address.ResourceId] = result;
        } else {
            resourceResults = new() {
                [address.ResourceId] = result,
            };

            Results.Add(address.LibraryId, resourceResults);
        }
    }
    
    private bool TryGetResult(LibraryID libraryId, ResourceID resourceId, out ResourceBuildingResult result) {
        if (!Results.TryGetValue(libraryId, out var libraryResults)) {
            result = default;
            return false;
        }
        
        return libraryResults.TryGetValue(resourceId, out result);
    }

    private bool TryGetResult(ResourceAddress address, out ResourceBuildingResult result) {
        if (!Results.TryGetValue(address.LibraryId, out var libraryResults)) {
            result = default;
            return false;
        }
        
        return libraryResults.TryGetValue(address.ResourceId, out result);
    }
    
    private void SetResult(LibraryID libraryId, ResourceID resourceId, ResourceBuildingResult result) {
        if (Results.TryGetValue(libraryId, out var resourceResults)) {
            resourceResults[resourceId] = result;
        } else {
            resourceResults = new() {
                [resourceId] = result,
            };

            Results.Add(libraryId, resourceResults);
        }
    }

    private void SerializeProcessedObject(
        ContentRepresentation processed,
        ResourceAddress address,
        IImportOptions? options,
        IReadOnlyCollection<string> tags
    ) {
        var factory = _environment.SerializerFactories.GetSerializableFactory(processed.GetType())!;
        
        if (factory == null) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializerFactory, processed.GetType()));
        }

        using MemoryStream ms = _environment.MemoryStreamManager.GetStream($"BuildSession L{address.LibraryId}-R{address.ResourceId}");

        var serializer = factory.InternalCreateSerializer(processed, new(options, _environment.Logger));
        
        CompileHelpers.Compile(serializer, ms, tags);
        ms.Position = 0;
        _environment.Output.CopyCompiledResourceOutput(ms, address);
    }

    private void AppendProceduralResources(
        ResourceAddress sourceResourceAddress,
        ProceduralResourceCollection proceduralResources,
        Dictionary<ResourceAddress, ProceduralResourceRequest> receiver
    ) {
        foreach ((var resourceId, var resource) in proceduralResources) {
            receiver.Add(new(sourceResourceAddress.LibraryId, resourceId), new(sourceResourceAddress.ResourceId, resource));
        }
    }
    
    /// <summary>
    /// Determines whether a resource should be rebuilt based on timeline, configurations, dependencies from previous build informations.
    /// </summary>
    /// <param name="address">Resource to determines whether rebuilding needed.</param>
    /// <param name="sourceLastWriteTimes">The last write times of resource's sources.</param>
    /// <param name="currentOptions">Building options of the resource.</param>
    /// <param name="currentDependencies">Dependencies of the resource.</param>
    /// <param name="previousIncrementalInfo">
    ///     When this method returns, contains the <see cref="IncrementalInfo"/> of the previous building session of the resource. If the
    ///     resource hasn't been build before, <see langword="default"/> is returned.
    /// </param>
    /// <returns><see langword="true"/> if the resource should be rebuilt; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The function does not account for the version of building components.</remarks>
    private bool IsResourceCacheable(
        ResourceAddress address,
        SourceLastWriteTimes sourceLastWriteTimes,
        BuildingOptions currentOptions,
        IReadOnlySet<ResourceAddress> currentDependencies,
        out IncrementalInfo previousIncrementalInfo
    ) {
        if (!_environment.IncrementalInfos.TryGetValue(address.LibraryId, out var libraryIncrementalInfos)) {
            previousIncrementalInfo = default;
            return false;
        }
        
        // If resource has been built before, and have old report, we can begin checking for caching.
        if (_environment.Output.GetResourceLastBuildTime(address) is { } resourceLastBuildTime &&
            libraryIncrementalInfos.TryGetValue(address.ResourceId, out previousIncrementalInfo)) {
            if (CompareLastWriteTimes(previousIncrementalInfo.SourcesLastWriteTime, sourceLastWriteTimes)) {
                // If the options are equal, no need to rebuild.
                if (currentOptions.Equals(previousIncrementalInfo.Options)) {
                    if (previousIncrementalInfo.DependencyAddresses.SequenceEqual(currentDependencies)) {
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
        public IReadOnlySet<ResourceAddress> DependencyResourceAddresses;
        public readonly ResourceRegistry.Element<BuildingResource> RegistryElement;
        
        public ContentRepresentation? ImportOutput { get; private set; }
        
        public int ReferenceCount;

        public EnvironmentResourceVertex(
            BuildResourceLibrary library,
            IReadOnlySet<ResourceAddress> dependencyResourceAddresses,
            ResourceRegistry.Element<BuildingResource> registryElement
        ) {
            Library = library;
            DependencyResourceAddresses = dependencyResourceAddresses;
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

    private readonly record struct ProceduralResourceRequest(ResourceID SourceResourceId, BuildingProceduralResource Resource);
}
// ReSharper disable VariableHidesOuterVariable

// TODO:
// 04-08-2025: Using ProceduralSchematic, detect if the compiled resource file missing to trigger compilation of that resource.
               
using Caxivitual.Lunacub.Collections;
using Microsoft.IO;

namespace Caxivitual.Lunacub.Building;

internal sealed partial class BuildSession {
    private readonly BuildEnvironment _environment;

    public LibraryIdentityDictionary<ResourceResultDictionary> Results { get; }
    
    private readonly FrozenDictionary<LibraryID, LibraryGraphVertices> _graph;
    
    private readonly Dictionary<LibraryID, ResourceRegistry<ResourceRegistry.Element>> _outputRegistries;
    private readonly EnvironmentProceduralSchematic _overrideProceduralSchematic;
    
    private readonly Dictionary<ResourceAddress, ProceduralResourceRequest> _proceduralResources;
    
    public BuildSession(BuildEnvironment environment) {
        _environment = environment;
        _graph = _environment.Libraries.Select(x => {
            Dictionary<ResourceID, ResourceVertex> vertices = new(x.Registry.Count);
            
            return KeyValuePair.Create<LibraryID, LibraryGraphVertices>(x.Id, new(x, vertices));
        }).ToFrozenDictionary();
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
    }

    private void ReleaseDependencies(IReadOnlyCollection<ResourceAddress> dependencyAddresses) {
        foreach (var dependencyAddress in dependencyAddresses) {
            if (!_graph.TryGetValue(dependencyAddress.LibraryId, out var libraryVertices)) continue;
            if (!libraryVertices.Vertices.TryGetValue(dependencyAddress.ResourceId, out ResourceVertex? vertex)) continue;

            ReleaseVertexOutput(vertex);
        }
    }

    private void ReleaseVertexOutput(ResourceVertex vertex) {
        if (vertex.DecrementReference() == 0) {
            vertex.DisposeImportedObject(new(_environment.Logger));
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

    private void SetResult(ResourceAddress address, FailureResult result) {
        if (Results.TryGetValue(address.LibraryId, out var resourceResults)) {
            resourceResults[address.ResourceId] = result;
        } else {
            resourceResults = new() {
                [address.ResourceId] = result,
            };

            Results.Add(address.LibraryId, resourceResults);
        }
    }
    
    private bool TryGetVertex(ResourceAddress address, [NotNullWhen(true)] out BuildResourceLibrary? library, [NotNullWhen(true)] out ResourceVertex? vertex) {
        if (_graph.TryGetValue(address.LibraryId, out var libraryVertices) &&
            libraryVertices.Vertices.TryGetValue(address.ResourceId, out vertex)
           ) {
            library = libraryVertices.Library;
            return true;
        }

        library = null;
        vertex = null;
        return false;
    }

    private bool TryGetVertex(ResourceAddress address, [NotNullWhen(true)] out ResourceVertex? vertex) {
        if (_graph.TryGetValue(address.LibraryId, out var libraryVertices) &&
            libraryVertices.Vertices.TryGetValue(address.ResourceId, out vertex)
        ) {
            return true;
        }

        vertex = null;
        return false;
    }
    
    private bool TryGetResult(LibraryID libraryId, ResourceID resourceId, out FailureResult result) {
        if (!Results.TryGetValue(libraryId, out var libraryResults)) {
            result = default;
            return false;
        }
        
        return libraryResults.TryGetValue(resourceId, out result);
    }

    private bool TryGetResult(ResourceAddress address, out FailureResult result) {
        if (!Results.TryGetValue(address.LibraryId, out var libraryResults)) {
            result = default;
            return false;
        }
        
        return libraryResults.TryGetValue(address.ResourceId, out result);
    }
    
    private void SetResult(LibraryID libraryId, ResourceID resourceId, FailureResult result) {
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
        object processed,
        ResourceAddress address,
        IImportOptions? options,
        IReadOnlyCollection<string> tags
    ) {
        var factory = _environment.SerializerFactories.GetSerializableFactory(processed.GetType())!;
        
        if (factory == null) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializerFactory, processed.GetType()));
        }

        using RecyclableMemoryStream ms = _environment.MemoryStreamManager.GetStream($"BuildOutput L{address.LibraryId}-R{address.ResourceId}");

        var serializer = factory.InternalCreateSerializer(processed, new(options, _environment.Logger, _environment.MemoryStreamManager));
        
        CompileHelpers.Compile(_environment, serializer, ms, tags);
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

    // /// <summary>
    // /// Determines whether a resource should be rebuilt based on timeline, configurations, dependencies from previous build informations.
    // /// </summary>
    // /// <param name="address">Resource to determines whether rebuilding needed.</param>
    // /// <param name="sourceLastWriteTimes">The last write times of resource's sources.</param>
    // /// <param name="currentOptions">Building options of the resource.</param>
    // /// <param name="currentDependencies">Dependencies of the resource.</param>
    // /// <param name="previousIncrementalInfo">
    // ///     When this method returns, contains the <see cref="IncrementalInfo"/> of the previous building session of the resource. If the
    // ///     resource hasn't been build before, <see langword="default"/> is returned.
    // /// </param>
    // /// <returns><see langword="true"/> if the resource should be rebuilt; otherwise, <see langword="false"/>.</returns>
    // /// <remarks>The function does not account for the version of building components.</remarks>
    // private bool IsResourceCacheable(
    //     ResourceAddress address,
    //     SourceLastWriteTimes sourceLastWriteTimes,
    //     BuildingOptions currentOptions,
    //     IReadOnlySet<ResourceAddress> currentDependencies,
    //     out IncrementalInfo previousIncrementalInfo
    // ) {
    //     if (!_environment.IncrementalInfos.TryGetValue(address.LibraryId, out var libraryIncrementalInfos)) {
    //         previousIncrementalInfo = default;
    //         return false;
    //     }
    //     
    //     // If resource has been built before, and have old report, we can begin checking for caching.
    //     if (_environment.Output.GetResourceLastBuildTime(address) is { } resourceLastBuildTime &&
    //         libraryIncrementalInfos.TryGetValue(address.ResourceId, out previousIncrementalInfo)) {
    //         if (CompareLastWriteTimes(previousIncrementalInfo.SourcesLastWriteTime, sourceLastWriteTimes)) {
    //             // If the options are equal, no need to rebuild.
    //             if (currentOptions.Equals(previousIncrementalInfo.Options)) {
    //                 if (previousIncrementalInfo.DependencyAddresses.SequenceEqual(currentDependencies)) {
    //                     return true;
    //                 }
    //             }
    //         }
    //
    //         return false;
    //     }
    //
    //     previousIncrementalInfo = default;
    //     return false;
    //
    //     // Check if resource's last write time is the same as the time stored in report.
    //     // Check if destination's last write time is later than resource's last write time.
    //     static bool CompareLastWriteTimes(SourceLastWriteTimes previous, SourceLastWriteTimes current) {
    //         if (previous.Primary != current.Primary) return false;
    //         if (previous.Secondaries == null) return current.Secondaries == null;
    //
    //         if (current.Secondaries == null) return false;
    //             
    //         foreach ((var previousSourceName, var previousSourceLastWriteTime) in previous.Secondaries) {
    //             if (!current.Secondaries.TryGetValue(previousSourceName, out var curentSourceLastWriteTime)) return false;
    //             if (previousSourceLastWriteTime != curentSourceLastWriteTime) return false;
    //         }
    //
    //         return true;
    //     }
    // }

    private sealed class ResourceVertex {
        /// <summary>
        /// Gets the Id of the dependency resources, the collection is unsanitied, thus it can reference unregistered resource
        /// id, or self-referencing.
        /// </summary>
        public IReadOnlySet<ResourceAddress> DependencyResourceAddresses { get; set; }
        
        /// <summary>
        /// Gets the <see cref="Importer"/> associate with the resource.
        /// </summary>
        public readonly Importer Importer;
        
        /// <summary>
        /// Gets the <see cref="Processor"/> associate with the resource.
        /// </summary>
        public readonly Processor? Processor;

        /// <summary>
        /// Gets the <see cref="SourcesInfo"/> associate with the resource.
        /// </summary>
        public readonly SourcesInfo SourcesInformation;
        
        /// <summary>
        /// Gets the data object imported by <see cref="Importer"/>.
        /// </summary>
        public object? ObjectRepresentation { get; private set; }
        
        public int ReferenceCount;

        public bool IsSelfUnchanged { get; set; }
        public bool IsBuilt { get; set; }

        public ResourceVertex(Importer importer, Processor? processor, SourcesInfo sourcesInfo) {
            Importer = importer;
            Processor = processor;
            SourcesInformation = sourcesInfo;
            DependencyResourceAddresses = FrozenSet<ResourceAddress>.Empty;
            ReferenceCount = 0;
        }
    
        [MemberNotNull(nameof(ObjectRepresentation))]
        public void SetObjectRepresentation(object obj) {
            ObjectRepresentation = obj;
        }

        public void IncrementReference() {
            Interlocked.Increment(ref ReferenceCount);
        }
        
        public int DecrementReference() {
            int original, computed;
            do {
                original = ReferenceCount;
                computed = ReferenceCount == 0 ? 0 : ReferenceCount - 1;
            } while (Interlocked.CompareExchange(ref ReferenceCount, computed, original) != original);

            return computed;
            // return Interlocked.Decrement(ref ReferenceCount);
        }
    
        public void DisposeImportedObject(DisposingContext context) {
            if (ObjectRepresentation == null) return;
            
            Debug.Assert(ReferenceCount == 0);
            
            Importer.Dispose(ObjectRepresentation, context);
            ObjectRepresentation = null;
        }
    }

    private readonly record struct ProceduralResourceRequest(ResourceID SourceResourceId, BuildingProceduralResource Resource);

    private readonly record struct LibraryGraphVertices(BuildResourceLibrary Library, Dictionary<ResourceID, ResourceVertex> Vertices);
}
// ReSharper disable VariableHidesOuterVariable

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

        // Dictionary<ResourceID, OutputRegistryElement> registry = Results
        //     .Where(x => x.Value.IsSuccess)
        //     .ToDictionary(kvp => kvp.Key, kvp => {
        //         var registryElement = _environment.Resources[kvp.Key];
        //         
        //         return new OutputRegistryElement(registryElement.Name, registryElement.Tags);
        //     });
        
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
    
    private bool Import(ResourceID rid, ResourceLibrary<BuildingResource> library, Importer importer, IImportOptions? options, [NotNullWhen(true)] out ContentRepresentation? imported, out ResourceBuildingResult failureResult) {
        Stream? resourceStream;

        try {
            if ((resourceStream = library.CreateResourceStream(rid)) is null) {
                Results.Add(rid, failureResult = new(BuildStatus.NullResourceStream));

                imported = null;
                return false;
            }
        } catch (InvalidResourceStreamException e) {
            Results.Add(rid, failureResult = new(BuildStatus.InvalidResourceStream, ExceptionDispatchInfo.Capture(e)));

            imported = null;
            return false;
        }

        try {
            ImportingContext context = new(options, _environment.Logger);
            imported = importer.ImportObject(resourceStream, context);

            failureResult = default;
            return true;
        } catch (Exception e) {
            Results.Add(rid, failureResult = new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                
            imported = null;
            return false;
        } finally {
            resourceStream.Dispose();
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

            Debug.Assert(!_environment.Libraries.ContainResource(hashedId), "ResourceID collided.");
            receiver.Add(hashedId, proceduralResource);
        }
    }
    
    /// <summary>
    /// Determines whether a resource should be rebuilt based on timeline, configurations, dependencies from previous build informations.
    /// </summary>
    /// <param name="rid">Resource to determines whether rebuilding needed.</param>
    /// <param name="resourceLastWriteTime">The last write time of resource.</param>
    /// <param name="currentOptions">Building options of the resource.</param>
    /// <param name="currentDependencies">Dependencies of the resource.</param>
    /// <param name="previousIncrementalInfo">
    ///     When this method returns, contains the <see cref="IncrementalInfo"/> of the previous building session of the resource. If the
    ///     resource hasn't been build before, <see langword="default"/> is returned.
    /// </param>
    /// <returns><see langword="true"/> if the resource should be rebuilt; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The function does not account for the version of building components.</remarks>
    private bool IsResourceCacheable(ResourceID rid, DateTime resourceLastWriteTime, BuildingOptions currentOptions, IReadOnlySet<ResourceID> currentDependencies, out IncrementalInfo previousIncrementalInfo) {
        // If resource has been built before, and have old report, we can begin checking for caching.
        if (_environment.Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && _environment.IncrementalInfos.TryGet(rid, out previousIncrementalInfo)) {
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousIncrementalInfo.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
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
    }

    private sealed class EnvironmentResourceVertex {
        public readonly BuildResourceLibrary Library;
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
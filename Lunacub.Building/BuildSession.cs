// ReSharper disable VariableHidesOuterVariable

using System.Collections.Frozen;
using System.IO.Hashing;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

internal sealed class BuildSession {
    private readonly BuildEnvironment _environment;

    private readonly Dictionary<ResourceID, ResourceVertex> _graph;
    
    public Dictionary<ResourceID, ResourceBuildingResult> Results { get; private set; }
    
    private readonly Dictionary<ResourceID, BuildingProceduralResource> _proceduralResources;

    public BuildSession(BuildEnvironment environment) {
        _environment = environment;
        _graph = new();
        Results = new();
        _proceduralResources = [];
    }

    public void Build() {
        foreach ((var rid, var resource) in _environment.Resources) {
            if (!_environment.Importers.TryGetValue(resource.Options.ImporterName, out var importer)) {
                Results.Add(rid, new(BuildStatus.UnknownImporter));
                continue;
            }
            
            // TODO: Add an optimization flag in Importer to skip this part.
            HashSet<ResourceID> dependencyIds;
            
            try {
                using (Stream stream = resource.Provider.GetStream()) {
                    dependencyIds = importer.ExtractDependencies(stream)
                        .Intersect(((IDictionary<ResourceID, BuildingResource>)_environment.Resources).Keys)
                        .ToHashSet();
                    dependencyIds.Remove(rid);   // Preventing self-referencing break everything.
                }
            } catch (Exception e) {
                Results.Add(rid, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                continue;
            }
            
            _graph.Add(rid, new(dependencyIds));
        }
        
        // Validate dependency graph.
        ValidateGraph();
        
        // Assigning reference count.
        foreach ((_, var vertex) in _graph) {
            foreach (var dependencyId in vertex.DependencyIds) {
                _graph[dependencyId].ReferenceCount++;
            }
        }
        
        // Handle the building procedure.
        foreach ((var rid, var vertex) in _graph) {
            BuildEnvironmentResource(rid, vertex, out _);
        }
        
        Debug.Assert(_graph.Values.All(x => x.ReferenceCount == 0));

        BuildProceduralResources();
    }

    private void ValidateGraph() {
        if (_graph.Count == 0) return;
        
        HashSet<ResourceID> temporaryMarks = [], permanentMarks = [];
        Stack<ResourceID> path = [];
            
        foreach ((var rid, _) in _graph) {
            Visit(rid, temporaryMarks, permanentMarks, path);
        }

        void Visit(ResourceID rid, HashSet<ResourceID> temporaryMarks, HashSet<ResourceID> permanentMarks, Stack<ResourceID> path) {
            if (permanentMarks.Contains(rid)) return;
                
            path.Push(rid);
                
            if (!temporaryMarks.Add(rid)) {
                throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
            }
                
            foreach (var dependencyID in _graph[rid].DependencyIds) {
                Visit(dependencyID, temporaryMarks, permanentMarks, path);
            }
                
            permanentMarks.Add(rid);
            path.Pop();
        }
    }

    private void BuildEnvironmentResource(ResourceID rid, ResourceVertex resourceVertex, out ResourceBuildingResult outputResult) {
        Debug.Assert(rid != ResourceID.Null);
        
        if (Results.TryGetValue(rid, out outputResult)) return;
        
        bool dependencyRebuilt = false;
        foreach (var dependencyId in resourceVertex.DependencyIds) {
            bool tryget = _graph.TryGetValue(dependencyId, out var dependencyVertexInfo);
            Debug.Assert(tryget);
            
            BuildEnvironmentResource(dependencyId, dependencyVertexInfo!, out ResourceBuildingResult dependencyResult);

            if (dependencyResult.Status == BuildStatus.Success) {
                dependencyRebuilt = true;
            }
        }

        try {
            (ResourceProvider provider, BuildingOptions options) = _environment.Resources[rid];
            DateTime resourceLastWriteTime = provider.LastWriteTime;

            Importer importer = _environment.Importers[options.ImporterName];
            
            Processor? processor;
            if (string.IsNullOrEmpty(options.ProcessorName)) {
                processor = Processor.Passthrough;
            } else if (!_environment.Processors.TryGetValue(options.ProcessorName, out processor)) {
                Results.Add(rid, outputResult = new(BuildStatus.UnknownProcessor));
                return;
            }
        
            if (!dependencyRebuilt && IsResourceCacheable(rid, resourceLastWriteTime, options, resourceVertex.DependencyIds, out IncrementalInfo previousIncrementalInfo)) {
                if (new ComponentVersions(importer.Version, processor.Version) == previousIncrementalInfo.ComponentVersions) {
                    Results.Add(rid, outputResult = new(BuildStatus.Cached));
                    
                    ReleaseDependencies(resourceVertex.DependencyIds);
                    return;
                }
            }
            
            // Import if haven't.
            if (resourceVertex.ImportOutput == null) {
                if (!Import(rid, provider, options, out ContentRepresentation? importOutput, out ImportingContext? context, out outputResult)) {
                    return;
                }
                
                resourceVertex.SetImportResult(importOutput, context);
            }
            
            // Collect dependencies.
            IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies = CollectDependencies(resourceVertex.DependencyIds);

            try {
                if (!Process(rid, resourceVertex.ImportOutput, options, dependencies, out ContentRepresentation? processed, out ProcessingContext? processingContext, out outputResult)) {
                    return;
                }

                try {
                    SerializeProcessedObject(processed, rid, options);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    processed.Dispose();
                }

                Results.Add(rid, outputResult = new(BuildStatus.Success));
                _environment.IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, resourceVertex.DependencyIds, new(importer.Version, processor.Version)));

                if (processingContext != null) {
                    AppendProceduralResources(rid, processingContext.ProceduralResources);
                }
            } finally {
                ReleaseDependencies(resourceVertex.DependencyIds);
            }
            
            Debug.Assert(resourceVertex.ImportingContext != null);
        
            foreach (var referenceId in resourceVertex.ImportingContext.ReferenceIds) {
                if (!_graph.TryGetValue(referenceId, out var referenceResourceVertex)) continue;

                BuildEnvironmentResource(referenceId, referenceResourceVertex, out _);
            }
        } finally {
            resourceVertex.Release();
        }
    }

    private void BuildProceduralResources() {
        if (_proceduralResources.Count == 0) return;
        
        // TODO: Implementation
    }
    
    private IReadOnlyDictionary<ResourceID, ContentRepresentation> CollectDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
        if (dependencyIds.Count == 0) return FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
        
        Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];

        foreach (var dependencyId in dependencyIds) {
            ResourceVertex dependencyVertex = _graph[dependencyId];

            if (Results.TryGetValue(dependencyId, out var result)) {
                switch (result.Status) {
                    case BuildStatus.Success:
                        Debug.Assert(dependencyVertex.ImportOutput != null);
                        
                        dependencyCollection[dependencyId] = dependencyVertex.ImportOutput;
                        continue;
                    
                    case BuildStatus.Cached: break; // Import the resource.
                    
                    default: continue;
                }
            }

            (ResourceProvider provider, BuildingOptions options) = _environment.Resources[dependencyId];

            if (Import(dependencyId, provider, options, out ContentRepresentation? imported, out ImportingContext? context, out _)) {
                dependencyVertex.SetImportResult(imported, context);
                dependencyCollection.Add(dependencyId, imported);
            }
        }

        return dependencyCollection;
    }
    
    private void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
        foreach (var dependencyId in dependencyIds) {
            _graph[dependencyId].Release();
        }
    }
    
    private bool Import(ResourceID rid, ResourceProvider provider, BuildingOptions options, [NotNullWhen(true)] out ContentRepresentation? imported, [NotNullWhen(true)] out ImportingContext? context, out ResourceBuildingResult failureResult) {
        using (Stream stream = provider.GetStream()) {
            if (stream is not { CanRead: true, CanSeek: true }) {
                Results.Add(rid, failureResult = new(BuildStatus.InvalidResourceStream));

                context = null;
                imported = null;
                return false;
            }

            try {
                context = new(options.Options);
                imported = _environment.Importers[options.ImporterName].ImportObject(stream, context);

                failureResult = default;
                return true;
            } catch (Exception e) {
                Results.Add(rid, failureResult = new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                
                context = null;
                imported = null;
                return false;
            }
        }
    }
    
    private bool Process(ResourceID rid, ContentRepresentation imported, BuildingOptions options, IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies, [NotNullWhen(true)] out ContentRepresentation? processed, out ProcessingContext? context, out ResourceBuildingResult failureResult) {
        string? processorName = options.ProcessorName;

        if (string.IsNullOrEmpty(processorName)) {
            processed = imported;
            context = null;
            failureResult = default;
            return true;
        }
        
        if (!_environment.Processors.TryGetValue(processorName, out var processor)) {
            Results.Add(rid, failureResult = new(BuildStatus.UnknownProcessor));
            processed = null;
            context = null;
            return false;
        }

        try {
            context = new(_environment, options.Options, dependencies, _environment.Logger);
            processed = processor.Process(imported, context);

            failureResult = default;
            return true;
        } catch (Exception e) {
            Results.Add(rid, failureResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
        
            processed = null;
            context = null;
            return false;
        }
    }

    private void SerializeProcessedObject(ContentRepresentation processed, ResourceID rid, BuildingOptions options) {
        var factory = _environment.SerializerFactories.GetSerializableFactory(processed.GetType())!;
        
        if (factory == null) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializerFactory, processed.GetType()));
        }

        using MemoryStream ms = new(4096);

        var serializer = factory.InternalCreateSerializer(processed, new(options.Options, _environment.Logger));
        
        CompileHelpers.Compile(serializer, ms, options.Tags);
        ms.Position = 0;
        _environment.Output.CopyCompiledResourceOutput(ms, rid);
    }

    private void AppendProceduralResources(ResourceID rid, IReadOnlyDictionary<ProceduralResourceID, BuildingProceduralResource> generatedResources) {
        unsafe {
            Span<byte> buffer = stackalloc byte[sizeof(ResourceID) + sizeof(ProceduralResourceID)];
            
            MemoryMarshal.Write(buffer, rid);
            Span<byte> slice = buffer[sizeof(ResourceID)..];

            foreach ((var proceduralId, var proceduralResource) in generatedResources) {
                MemoryMarshal.Write(slice, proceduralId);

                UInt128 hash = XxHash128.HashToUInt128(buffer);

                Debug.Assert(!_environment.Resources.ContainsKey(hash), "ResourceID collided.");
                _proceduralResources.Add(hash, proceduralResource);
            }
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

    private sealed class ResourceVertex {
        public readonly IReadOnlySet<ResourceID> DependencyIds;

        public ContentRepresentation? ImportOutput { get; private set; }
        public ImportingContext? ImportingContext { get; private set; }
        
        public int ReferenceCount;

        public ResourceVertex(IReadOnlySet<ResourceID> dependencies) {
            DependencyIds = dependencies;
            ReferenceCount = 1;
        }

        [MemberNotNull(nameof(ImportOutput), nameof(ImportingContext))]
        public void SetImportResult(ContentRepresentation importOutput, ImportingContext context) {
            Debug.Assert(ReferenceCount > 0);
            
            ImportOutput = importOutput;
            ImportingContext = context;
        }
        
        public void Release() {
            if (--ReferenceCount != 0) return;

            ImportOutput?.Dispose();
            ImportOutput = null;
            ImportingContext = null;
        }
    }
}
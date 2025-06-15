// ReSharper disable VariableHidesOuterVariable

using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

internal sealed class BuildSession {
    private readonly BuildEnvironment _environment;

    private readonly Dictionary<ResourceID, ResourceVertex> _graph;
    
    public Dictionary<ResourceID, ResourceBuildingResult> Results { get; }
    
    public BuildSession(BuildEnvironment environment) {
        _environment = environment;
        _graph = new(_environment.Resources.Count);
        Results = new();
    }

    public void Build() {
        BuildEnvironmentResources(out var proceduralResources);
        BuildProceduralResources(proceduralResources);
        
        Debug.Assert(_graph.Values.All(x => x.ImportOutput == null || IsDisposed(x.ImportOutput)));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_disposed")]
    static extern ref bool IsDisposed(ContentRepresentation contentRepresentation);

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

    private void BuildEnvironmentResources(out Dictionary<ResourceID, BuildingProceduralResource> proceduralResources) {
        _environment.Logger.LogInformation("Building environment resources.");
        
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
        
        // Assigning reference count for dependencies.
        foreach ((_, var vertex) in _graph) {
            foreach (var dependencyId in vertex.DependencyIds) {
                _graph[dependencyId].ReferenceCount++;
            }

            vertex.ReferenceCount++;    // Environment resources need the import output to process and serialize.
        }

        proceduralResources = [];
        
        // Handle the building procedure.
        foreach ((var rid, var vertex) in _graph) {
            BuildEnvironmentResource(rid, vertex, proceduralResources, out _);
        }
        
        Debug.Assert(_graph.Values.All(x => x.ReferenceCount == 0));
    }
    private void BuildEnvironmentResource(ResourceID rid, ResourceVertex resourceVertex, Dictionary<ResourceID, BuildingProceduralResource> proceduralResources, out ResourceBuildingResult outputResult) {
        Debug.Assert(rid != ResourceID.Null);
        
        if (Results.TryGetValue(rid, out outputResult)) return;
        
        bool dependencyRebuilt = false;
        foreach (var dependencyId in resourceVertex.DependencyIds) {
            bool tryget = _graph.TryGetValue(dependencyId, out var dependencyVertexInfo);
            Debug.Assert(tryget);
            
            BuildEnvironmentResource(dependencyId, dependencyVertexInfo!, proceduralResources, out ResourceBuildingResult dependencyResult);

            if (dependencyResult.Status == BuildStatus.Success) {
                dependencyRebuilt = true;
            }
        }

        try {
            (ResourceProvider provider, BuildingOptions options) = _environment.Resources[rid];
            DateTime resourceLastWriteTime = provider.LastWriteTime;

            Importer importer = _environment.Importers[options.ImporterName];

            Processor? processor = null;

            if (!string.IsNullOrEmpty(options.ProcessorName) && !_environment.Processors.TryGetValue(options.ProcessorName, out processor)) {
                Results.Add(rid, outputResult = new(BuildStatus.UnknownProcessor));
                return;
            }
            
            if (!dependencyRebuilt && IsResourceCacheable(rid, resourceLastWriteTime, options, resourceVertex.DependencyIds, out IncrementalInfo previousIncrementalInfo)) {
                if (new ComponentVersions(importer.Version, processor?.Version) == previousIncrementalInfo.ComponentVersions) {
                    Results.Add(rid, outputResult = new(BuildStatus.Cached));
                    return;
                }
            }
            
            // Import if haven't.
            if (resourceVertex.ImportOutput == null) {
                if (!Import(rid, provider, importer, options.Options, out ContentRepresentation? importOutput, out outputResult)) {
                    return;
                }
                
                resourceVertex.SetImportResult(importOutput);
            }

            if (processor == null) {
                try {
                    SerializeProcessedObject(resourceVertex.ImportOutput, rid, options.Options, options.Tags);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
            } else {
                if (!processor.CanProcess(resourceVertex.ImportOutput)) {
                    Results.Add(rid, outputResult = new(BuildStatus.Unprocessable));
                    return;
                }

                ContentRepresentation processed;
                IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies = CollectDependencies(resourceVertex.DependencyIds);
                ProcessingContext processingContext;

                try {
                    processingContext = new(_environment, options.Options, dependencies, _environment.Logger);
                    processed = processor.Process(resourceVertex.ImportOutput, processingContext);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }

                try {
                    SerializeProcessedObject(processed, rid, options.Options, options.Tags);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
                        processed.Dispose();
                    }
                }
                
                AppendProceduralResources(rid, processingContext.ProceduralResources, proceduralResources);
            }

            Results.Add(rid, outputResult = new(BuildStatus.Success));
            _environment.IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, resourceVertex.DependencyIds, new(importer.Version, processor?.Version)));
        } finally {
            ReleaseDependencies(resourceVertex.DependencyIds);
            resourceVertex.Release();
        }
        
        IReadOnlyDictionary<ResourceID, ContentRepresentation> CollectDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
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

                if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                    dependencyCollection.Add(dependencyId, dependencyImportOutput);
                } else {
                    (ResourceProvider provider, BuildingOptions options) = _environment.Resources[dependencyId];

                    if (Import(dependencyId, provider, _environment.Importers[options.ImporterName], options.Options, out ContentRepresentation? imported, out _)) {
                        dependencyVertex.SetImportResult(imported);
                        dependencyCollection.Add(dependencyId, imported);
                    }
                }
            }

            return dependencyCollection;
        }
    }

    private void BuildProceduralResources(IReadOnlyDictionary<ResourceID, BuildingProceduralResource> proceduralResources) {
        if (proceduralResources.Count == 0) return;

        new ProceduralBuildLayer(this, null, proceduralResources).Build();

        // foreach ((var rid, var resource) in proceduralResources) {
        //     ResourceVertex vertex = new(resource.DependencyIds);
        //     vertex.SetImportResult(resource.Object);
        //     
        //     _graph.Add(rid, vertex);
        // }
        //
        // ValidateGraph();
        //
        // // Assigning reference count for dependencies.
        // foreach ((_, var vertex) in _graph) {
        //     foreach (var dependencyId in vertex.DependencyIds) {
        //         _graph[dependencyId].ReferenceCount++;
        //     }
        // }
        //
        // Dictionary<ResourceID, BuildingProceduralResource> nextPassProceduralResources = [];
        //
        // foreach ((var rid, var vertex) in _graph) {
        //     BuildProceduralResource(rid, vertex, proceduralResources, nextPassProceduralResources, pass, out _);
        // }
        //
        // Debug.Assert(_graph.Values.All(x => x.ReferenceCount == 0));
        //
        // BuildProceduralResources(nextPassProceduralResources, pass + 1);
    }
    
    private void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
        foreach (var dependencyId in dependencyIds) {
            _graph[dependencyId].Release();
        }
    }
    
    private bool Import(ResourceID rid, ResourceProvider provider, Importer importer, IImportOptions? options, [NotNullWhen(true)] out ContentRepresentation? imported, out ResourceBuildingResult failureResult) {
        using (Stream stream = provider.GetStream()) {
            if (stream is not { CanRead: true, CanSeek: true }) {
                Results.Add(rid, failureResult = new(BuildStatus.InvalidResourceStream));

                imported = null;
                return false;
            }

            try {
                ImportingContext context = new(options);
                imported = importer.ImportObject(stream, context);

                failureResult = default;
                return true;
            } catch (Exception e) {
                Results.Add(rid, failureResult = new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                
                imported = null;
                return false;
            }
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
        unsafe {
            Span<byte> buffer = stackalloc byte[sizeof(ResourceID) + sizeof(ProceduralResourceID)];
            
            MemoryMarshal.Write(buffer, rid);
            Span<byte> slice = buffer[sizeof(ResourceID)..];

            foreach ((var proceduralId, var proceduralResource) in proceduralResources) {
                MemoryMarshal.Write(slice, proceduralId);

                UInt128 hash = XxHash128.HashToUInt128(buffer);

                Debug.Assert(!_environment.Resources.ContainsKey(hash), "ResourceID collided.");
                receiver.Add(hash, proceduralResource);
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
        
        public int ReferenceCount;

        public ResourceVertex(IReadOnlySet<ResourceID> dependencies) {
            DependencyIds = dependencies;
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

    private sealed class ProceduralBuildLayer {
        private readonly BuildSession _session;
        private readonly ProceduralBuildLayer? _previous;

        private readonly IReadOnlyDictionary<ResourceID, BuildingProceduralResource> _proceduralResources;
        
        public Dictionary<ResourceID, BuildingProceduralResource> NextLayerProceduralResources { get; }

        public ProceduralBuildLayer(BuildSession session, ProceduralBuildLayer? previousLayer, IReadOnlyDictionary<ResourceID, BuildingProceduralResource> proceduralResources) {
            _session = session;
            _previous = previousLayer;
            _proceduralResources = proceduralResources;
            NextLayerProceduralResources = [];
        }

        public ProceduralBuildLayer Build() {
            if (_proceduralResources.Count == 0) return this;
            
            foreach ((ResourceID rid, BuildingProceduralResource resource) in _proceduralResources) {
                HashSet<ResourceID> sanitizedDependencies = new(resource.DependencyIds.Count);

                foreach (var dependencyId in resource.DependencyIds) {
                    if (_session._graph.ContainsKey(dependencyId) || _proceduralResources.ContainsKey(dependencyId)) {
                        if (dependencyId != rid) {
                            sanitizedDependencies.Add(dependencyId);
                        }
                    }
                }
                
                ResourceVertex vertex = new(sanitizedDependencies);
                vertex.SetImportResult(resource.Object);
            
                _session._graph.Add(rid, vertex);
            }

            try {
                _session.ValidateGraph();

                // Not gonna do reference counting here because procedural resources are only created one, and it's very hard
                // to have an API that keeping track of refcount and releasing.
                
                foreach ((var rid, var vertex) in _proceduralResources) {
                    BuildResource(rid, vertex, out _);
                }

                return new ProceduralBuildLayer(_session, this, NextLayerProceduralResources).Build();
            } finally {
                foreach ((_, BuildingProceduralResource resource) in _proceduralResources) {
                    resource.Object.Dispose();
                }
            }
        }
        
        private void BuildResource(ResourceID rid, BuildingProceduralResource resource, out ResourceBuildingResult outputResult) {
            Debug.Assert(rid != ResourceID.Null);
            Debug.Assert(resource.Object != null);

            var resourceVertex = _session._graph[rid];
            
            Debug.Assert(ReferenceEquals(resource.Object, resourceVertex.ImportOutput));
            
            try {
                if (string.IsNullOrEmpty(resource.ProcessorName)) {
                    try {
                        _session.SerializeProcessedObject(resourceVertex.ImportOutput, rid, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                } else if (_session._environment.Processors.TryGetValue(resource.ProcessorName, out var processor)) {
                    if (!processor.CanProcess(resourceVertex.ImportOutput)) {
                        _session.Results.Add(rid, outputResult = new(BuildStatus.Unprocessable));
                        return;
                    }

                    ContentRepresentation processed;
                    IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies = CollectDependencies(resourceVertex.DependencyIds);
                    ProcessingContext processingContext;
                    
                    try {
                        processingContext = new(_session._environment, resource.Options, dependencies, _session._environment.Logger);
                        processed = processor.Process(resourceVertex.ImportOutput, processingContext);
                    } catch (Exception e) {
                        _session.Results.Add(rid, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }

                    try {
                        _session.SerializeProcessedObject(processed, rid, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    } finally {
                        if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
                            processed.Dispose();
                        }
                    }
                    
                    _session.AppendProceduralResources(rid, processingContext.ProceduralResources, NextLayerProceduralResources);
                } else {
                    _session.Results.Add(rid, outputResult = new(BuildStatus.UnknownProcessor));
                    return;
                }
                
                _session.Results.Add(rid, outputResult = new(BuildStatus.Success));
            } finally {
                ReleaseDependencies(resource.DependencyIds);
            }
            
            IReadOnlyDictionary<ResourceID, ContentRepresentation> CollectDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
                if (dependencyIds.Count == 0) return FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
            
                Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];

                foreach (var dependencyId in dependencyIds) {
                    ResourceVertex dependencyVertex = _session._graph[dependencyId];

                    if (_session.Results.TryGetValue(dependencyId, out var result)) {
                        switch (result.Status) {
                            case BuildStatus.Success:
                                Debug.Assert(dependencyVertex.ImportOutput != null);
                            
                                dependencyCollection[dependencyId] = dependencyVertex.ImportOutput;
                                continue;
                        
                            case BuildStatus.Cached: break; // Import the resource.
                        
                            default: continue;
                        }
                    }

                    if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                        dependencyCollection.Add(dependencyId, dependencyImportOutput);
                    } else {
                        if (_session._environment.Resources.TryGetValue(dependencyId, out BuildingResource envResource)) {
                            (ResourceProvider provider, BuildingOptions options) = envResource;
                                
                            if (_session.Import(dependencyId, provider, _session._environment.Importers[options.ImporterName], options.Options, out ContentRepresentation? imported, out _)) {
                                dependencyVertex.SetImportResult(imported);
                                dependencyCollection.Add(dependencyId, imported);
                            }
                        } else if (_proceduralResources.TryGetValue(dependencyId, out var proceduralResource)) {
                            Debug.Assert(_session._graph[dependencyId].ReferenceCount > 0);
                            
                            dependencyCollection.Add(dependencyId, proceduralResource.Object);
                        }
                    }
                }

                return dependencyCollection;
            }
        }
        
        private void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
            foreach (var dependencyId in dependencyIds) {
                if (!_session._environment.Resources.ContainsKey(dependencyId)) continue;   // Only release environment resources.
                
                _session._graph[dependencyId].Release();
            }
        }
    }
}
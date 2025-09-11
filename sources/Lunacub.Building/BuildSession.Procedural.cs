// ReSharper disable VariableHidesOuterVariable

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildProceduralResources() {
        if (_proceduralResources.Count != 0) {
            Log.BeginBuildingProceduralResources(_environment.Logger);
            Log.ProceduralResourcesDetected(_environment.Logger, _proceduralResources.Count);
            
            new ProceduralResourceBuild(this, _proceduralResources, null, 0).Build();
        }

        Log.FinishBuildingResources(_environment.Logger);
        
        // We're done building every resources.
        ProcessPostBuild();
    }

    private sealed class ProceduralResourceBuild {
        private readonly BuildSession _session;
        private readonly Dictionary<ResourceAddress, BuildingProceduralResource> _current;
        private readonly Dictionary<ResourceAddress, BuildingProceduralResource> _next;
        private readonly ProceduralResourceBuild? _previous;
        private readonly int _layer;

        public ProceduralResourceBuild(
            BuildSession session,
            Dictionary<ResourceAddress, BuildingProceduralResource> current,
            ProceduralResourceBuild? previous,
            int layer
        ) {
            _session = session;
            _current = current;
            _next = [];
            _previous = previous;
            _layer = layer;
        }

        public ProceduralResourceBuild Build() {
            Debug.Assert(_current.Count > 0);

            ILogger logger = _session._environment.Logger;
            
            Log.BeginBuildingProceduralResources(logger, _layer);
            Log.ValidatingDependencyGraph(logger);
            
            CheckCycle();
            
            Log.CountEnvironmentResourcesReferenceCount(logger);
            
            // Increment reference counts for environment vertices.
            foreach ((_, var proceduralResource) in _current) {
                foreach (var dependencyAddress in proceduralResource.DependencyAddresses) {
                    if (!_session._graph.TryGetValue(dependencyAddress.LibraryId, out var libraryVertices)) continue;
                    if (!libraryVertices.Library.Registry.ContainsKey(dependencyAddress.ResourceId)) continue;

                    if (libraryVertices.Vertices.TryGetValue(dependencyAddress.ResourceId, out var vertex)) {
                        vertex.IncrementReference();
                    }
                }
            }
            
            Log.CompileProceduralResources(logger);
            
            foreach ((var resourceAddress, var proceduralResource) in _current) {
                BuildProceduralResource(resourceAddress, proceduralResource);
            }
            
            try {
                if (_next.Count == 0) return this;
            
                return new ProceduralResourceBuild(_session, _next, this, _layer + 1).Build();
            } finally {
                foreach ((_, var proceduralResource) in _current) {
                    proceduralResource.Disposer?.Invoke(proceduralResource.Object);
                }
            }
        }

        private void CheckCycle() {
            // Only have to validate _current for cyclic dependency.

            HashSet<ResourceAddress> temporaryMarks = [], permanentMarks = [];
            Stack<ResourceAddress> path = [];

            foreach ((var address, _) in _current) {
                Visit(address, temporaryMarks, permanentMarks, path);
            }

            void Visit(
                ResourceAddress address,
                HashSet<ResourceAddress> temporaryMarks,
                HashSet<ResourceAddress> permanentMarks,
                Stack<ResourceAddress> path
            ) {
                if (permanentMarks.Contains(address)) return;

                path.Push(address);

                if (!temporaryMarks.Add(address)) {
                    string message = string.Format(ExceptionMessages.CircularResourceDependency, string.Join(" -> ", path.Reverse()));
                    throw new InvalidOperationException(message);
                }

                foreach (var dependencyID in _current[address].DependencyAddresses) {
                    if (!_current.ContainsKey(address)) continue;

                    Visit(dependencyID, temporaryMarks, permanentMarks, path);
                }

                permanentMarks.Add(address);
                path.Pop();
            }
        }

        private void BuildProceduralResource(ResourceAddress resourceAddress, BuildingProceduralResource proceduralResource) {
            BuildEnvironment environment = _session._environment;
            
            try {
                string? processorName = proceduralResource.ProcessorName;

                if (string.IsNullOrEmpty(processorName)) {
                    try {
                        _session.SerializeProcessedObject(proceduralResource.Object, resourceAddress, proceduralResource.Options, proceduralResource.Tags);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                } else {
                    if (!environment.Processors.TryGetValue(processorName, out var processor)) {
                        _session.SetResult(resourceAddress, new(BuildStatus.UnknownProcessor));
                        return;
                    }
                    
                    if (!processor.CanProcess(proceduralResource.Object)) {
                        _session.SetResult(resourceAddress, new(BuildStatus.Unprocessable));
                        return;
                    }
                    
                    CollectDependencies(proceduralResource.DependencyAddresses, out var dependencies);
                    
                    object processed;
                    ProcessingContext processingContext;
                    
                    try {
                        processingContext = new(environment, resourceAddress, proceduralResource.Options, dependencies, environment.Logger);
                        processed = processor.Process(proceduralResource.Object, processingContext);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                    
                    try {
                        _session.SerializeProcessedObject(processed, resourceAddress, proceduralResource.Options, proceduralResource.Tags);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    } finally {
                        if (!ReferenceEquals(proceduralResource.Object, processed)) {
                            processor.Dispose(processed, new(_session._environment.Logger));
                        }
                    }
                    
                    AppendProceduralResources(resourceAddress, processingContext.ProceduralResources, _next);
                }
                
                _session.AddOutputResourceRegistry(resourceAddress, new(null, proceduralResource.Tags));
            } finally {
                _session.ReleaseDependencies(proceduralResource.DependencyAddresses);
            }
        }
        
        private void CollectDependencies(
            IReadOnlySet<ResourceAddress> dependencyIds,
            out IReadOnlyDictionary<ResourceAddress, object> collectedDependencies
        ) {
            if (dependencyIds.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceAddress, object>.Empty;
                return;
            }
            
            Dictionary<ResourceAddress, object> dependencyCollection = [];

            foreach (var dependencyAddress in dependencyIds) {
                // If the dependency is environment's resource.
                if (_session.TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) {
                    if (dependencyVertex.ObjectRepresentation is { } dependencyImportOutput) {
                        dependencyCollection.Add(dependencyAddress, dependencyImportOutput);
                    } else {
                        BuildingOptions options = dependencyResourceLibrary.Registry[dependencyAddress.ResourceId].Option.Options;

                        if (_session.Import(
                                dependencyAddress,
                                dependencyResourceLibrary,
                                _session._environment.Importers[options.ImporterName],
                                options.Options,
                                out var imported,
                                out _)
                           ) {
                            dependencyVertex.SetObjectRepresentation(imported);
                            dependencyCollection.Add(dependencyAddress, imported);
                        }
                    }
                } else if (RecursivelyTryGetProceduralResource(dependencyAddress) is { } proceduralResource) {
                    dependencyCollection.Add(dependencyAddress, proceduralResource);
                }
            }

            collectedDependencies = dependencyCollection;
        }

        private object? RecursivelyTryGetProceduralResource(ResourceAddress resourceAddress) {
            if (_current.TryGetValue(resourceAddress, out BuildingProceduralResource resource)) {
                return resource.Object;
            }
            
            return _previous?.RecursivelyTryGetProceduralResource(resourceAddress);
        }
    }
}
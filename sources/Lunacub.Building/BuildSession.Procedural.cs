// ReSharper disable VariableHidesOuterVariable

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildProceduralResources() {
        if (_proceduralResources.Count != 0) {
            Log.BeginBuildingProceduralResources(_environment.Logger);
            new ProceduralResourceBuild(this, _proceduralResources, null).Build();
            
            Debug.Assert(
                _graph.Values.SelectMany(x => x.Vertices.Values).All(x => x.ObjectRepresentation == null),
                "Resource leaked after building procedural resources."
            );
        }

        Log.FinishBuildingResources(_environment.Logger);
        
        // We're done building every resources.
        // Now we should postprocess and flush some incremental infos like schematics.
        ProcessPostBuild();
    }

    private sealed class ProceduralResourceBuild {
        private readonly BuildSession _session;
        private readonly Dictionary<ResourceAddress, ProceduralResourceRequest> _current;
        private readonly ProceduralResourceBuild? _previous;

        public ProceduralResourceBuild(
            BuildSession session,
            Dictionary<ResourceAddress, ProceduralResourceRequest> current,
            ProceduralResourceBuild? previous
        ) {
            _session = session;
            _current = current;
            _previous = previous;
        }

        public ProceduralResourceBuild Build() {
            Debug.Assert(_current.Count > 0);

            ValidateProceduralResources();

            Dictionary<ResourceAddress, ProceduralResourceRequest> next = [];

            // Assign the reference count to environment resources.
            foreach ((_, var proceduralResource) in _current) {
                foreach (var dependencyAddress in proceduralResource.Resource.DependencyAddresses) {
                    if (!_session.TryGetVertex(dependencyAddress, out var dependencyVertex)) continue;
                    
                    dependencyVertex.IncrementReference();
                }
            }

            foreach ((var resourceAddress, var proceduralResource) in _current) {
                BuildProceduralResource(resourceAddress, proceduralResource, next);
            }

            try {
                if (next.Count == 0) return this;

                return new ProceduralResourceBuild(_session, next, this).Build();
            } finally {
                foreach ((_, var proceduralResource) in _current) {
                    proceduralResource.Resource.Disposer?.Invoke(proceduralResource.Resource.Object);
                }
            }
        }

        private void ValidateProceduralResources() {
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

                foreach (var dependencyID in _current[address].Resource.DependencyAddresses) {
                    if (!_current.ContainsKey(address)) continue;

                    Visit(dependencyID, temporaryMarks, permanentMarks, path);
                }

                permanentMarks.Add(address);
                path.Pop();
            }
        }

        private void BuildProceduralResource(
            ResourceAddress resourceAddress,
            ProceduralResourceRequest resourceRequest,
            Dictionary<ResourceAddress, ProceduralResourceRequest> next
        ) {
            if (_session.TryGetResult(resourceAddress, out _)) return;

            BuildEnvironment environment = _session._environment;

            (ResourceID sourceResourceId, BuildingProceduralResource resource) = resourceRequest;

            try {
                string? processorName = resource.ProcessorName;

                if (string.IsNullOrEmpty(processorName)) {
                    try {
                        _session.SerializeProcessedObject(resource.Object, resourceAddress, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                } else {
                    if (!environment.Processors.TryGetValue(processorName, out var processor)) {
                        _session.SetResult(resourceAddress, new(BuildStatus.UnknownProcessor));
                        return;
                    }
                    
                    if (!processor.CanProcess(resource.Object)) {
                        _session.SetResult(resourceAddress, new(BuildStatus.Unprocessable));
                        return;
                    }
                    
                    CollectDependencies(resource.DependencyAddresses, out var dependencies);
                    
                    object processed;
                    ProcessingContext processingContext;
                    
                    try {
                        processingContext = new(environment, resourceAddress, resource.Options, dependencies, environment.Logger);
                        processed = processor.Process(resource.Object, processingContext);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                    
                    try {
                        _session.SerializeProcessedObject(processed, resourceAddress, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    } finally {
                        if (!ReferenceEquals(resource.Object, processed)) {
                            processor.Dispose(processed, new(_session._environment.Logger));
                        }
                    }
                    
                    _session.AppendProceduralResources(resourceAddress, processingContext.ProceduralResources, next);
                }
                
                _session.AddOutputResourceRegistry(resourceAddress, new(null, resource.Tags));
                _session.AddOverrideProceduralSchematicEdge(new(resourceAddress.LibraryId, sourceResourceId), new(resourceAddress.ResourceId, resource.Tags));
            } finally {
                _session.ReleaseDependencies(resource.DependencyAddresses);
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
            if (_current.TryGetValue(resourceAddress, out ProceduralResourceRequest request)) return request.Resource.Object;
            
            return _previous?.RecursivelyTryGetProceduralResource(resourceAddress);
        }
    }
}
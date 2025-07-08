// ReSharper disable VariableHidesOuterVariable

using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildProceduralResources() {
        if (_proceduralResources.Count == 0) return;

        new ProceduralResourceBuild(this, _proceduralResources, null).Build();
    }

    private sealed class ProceduralResourceBuild {
        private readonly BuildSession _session;
        private readonly Dictionary<ResourceID, BuildingProceduralResource> _current;
        private readonly ProceduralResourceBuild? _previous;

        public ProceduralResourceBuild(
            BuildSession session,
            Dictionary<ResourceID,
            BuildingProceduralResource> current,
            ProceduralResourceBuild? previous
        ) {
            _session = session;
            _current = current;
            _previous = previous;
        }

        public ProceduralResourceBuild Build() {
            Debug.Assert(_current.Count > 0);

            ValidateProceduralResources();

            Dictionary<ResourceID, BuildingProceduralResource> next = [];

            // Assign the reference count to environment resources.
            foreach ((_, var proceduralResource) in _current) {
                foreach (var dependencyId in proceduralResource.DependencyIds) {
                    if (_session._graph.TryGetValue(dependencyId, out var environmentResourceVertex)) {
                        environmentResourceVertex.ReferenceCount++;
                    }
                }
            }

            foreach ((var resourceId, var proceduralResource) in _current) {
                BuildProceduralResource(resourceId, proceduralResource, next);
            }

            try {
                if (next.Count == 0) return this;

                return new ProceduralResourceBuild(_session, next, this).Build();
            } finally {
                foreach ((_, var proceduralResource) in _current) {
                    proceduralResource.Object.Dispose();
                }
            }
        }

        private void ValidateProceduralResources() {
            // Only have to validate _current for cyclic dependency.

            HashSet<ResourceID> temporaryMarks = [], permanentMarks = [];
            Stack<ResourceID> path = [];

            foreach ((var rid, _) in _current) {
                Visit(rid, temporaryMarks, permanentMarks, path);
            }

            void Visit(ResourceID rid, HashSet<ResourceID> temporaryMarks, HashSet<ResourceID> permanentMarks, Stack<ResourceID> path) {
                if (permanentMarks.Contains(rid)) return;

                path.Push(rid);

                if (!temporaryMarks.Add(rid)) {
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }

                foreach (var dependencyID in _current[rid].DependencyIds) {
                    if (!_current.ContainsKey(rid)) continue;

                    Visit(dependencyID, temporaryMarks, permanentMarks, path);
                }

                permanentMarks.Add(rid);
                path.Pop();
            }
        }

        private void BuildProceduralResource(
            ResourceID resourceId,
            BuildingProceduralResource resource,
            Dictionary<ResourceID, BuildingProceduralResource> next
        ) {
            Debug.Assert(resourceId != ResourceID.Null);
            
            if (_session.Results.ContainsKey(resourceId)) return;

            BuildEnvironment environment = _session._environment;

            try {
                string? processorName = resource.ProcessorName;

                if (string.IsNullOrEmpty(processorName)) {
                    try {
                        _session.SerializeProcessedObject(resource.Object, resourceId, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.Results.Add(resourceId, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                } else {
                    if (!environment.Processors.TryGetValue(processorName, out var processor)) {
                        _session.Results.Add(resourceId, new(BuildStatus.UnknownProcessor));
                        return;
                    }
                    
                    if (!processor.CanProcess(resource.Object)) {
                        _session.Results.Add(resourceId, new(BuildStatus.Unprocessable));
                        return;
                    }
                    
                    CollectDependencies(resource.DependencyIds, out IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies);
                    
                    ContentRepresentation processed;
                    ProcessingContext processingContext;
                    
                    try {
                        processingContext = new(environment, resourceId, resource.Options, dependencies, environment.Logger);
                        processed = processor.Process(resource.Object, processingContext);
                    } catch (Exception e) {
                        _session.Results.Add(resourceId, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                    
                    try {
                        _session.SerializeProcessedObject(processed, resourceId, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.Results.Add(resourceId, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    } finally {
                        if (!ReferenceEquals(resource.Object, processed)) {
                            processed.Dispose();
                        }
                    }
                    
                    _session.AppendProceduralResources(resourceId, processingContext.ProceduralResources, next);
                }
                
                _session.Results.Add(resourceId, new(BuildStatus.Success));
            } finally {
                _session.ReleaseDependencies(resource.DependencyIds);
            }
        }
        
        private void CollectDependencies(
            IReadOnlySet<ResourceID> dependencyIds,
            out IReadOnlyDictionary<ResourceID, ContentRepresentation> collectedDependencies
        ) {
            if (dependencyIds.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
                return;
            }
            
            Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];

            foreach (var dependencyId in dependencyIds) {
                if (_session._graph.TryGetValue(dependencyId, out EnvironmentResourceVertex? dependencyVertex)) {
                    if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                        dependencyCollection.Add(dependencyId, dependencyImportOutput);
                    } else {
                        if (_session.Results.TryGetValue(dependencyId, out var result)) {
                            // Traversed through this resource vertex before.
                            switch (result.Status) {
                                case BuildStatus.Success:
                                    Debug.Assert(dependencyVertex.ImportOutput != null);
                        
                                    dependencyCollection[dependencyId] = dependencyVertex.ImportOutput;
                                    continue;
                    
                                case BuildStatus.Cached:
                                    // Resource is cached, but still import it to handle dependencies.
                                    // Fallthrough.
                                    break;
                    
                                default: continue;
                            }
                    
                            BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;

                            if (_session.Import(
                                dependencyId,
                                dependencyVertex.Library,
                                _session._environment.Importers[options.ImporterName],
                                options.Options,
                                out var imported,
                                out _)
                            ) {
                                dependencyVertex.SetImportResult(imported);
                                dependencyCollection.Add(dependencyId, imported);
                            }
                        }
                    }
                } else if (RecursivelyTryGetProceduralResource(dependencyId) is { } proceduralResource) {
                    dependencyCollection.Add(dependencyId, proceduralResource);
                }
            }

            collectedDependencies = dependencyCollection;
        }

        private ContentRepresentation? RecursivelyTryGetProceduralResource(ResourceID resourceId) {
            if (_current.TryGetValue(resourceId, out BuildingProceduralResource resource)) return resource.Object;
            
            return _previous?.RecursivelyTryGetProceduralResource(resourceId);
        }
    }
}
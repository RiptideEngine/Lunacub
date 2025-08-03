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
        private readonly Dictionary<ResourceAddress, BuildingProceduralResource> _current;
        private readonly ProceduralResourceBuild? _previous;

        public ProceduralResourceBuild(
            BuildSession session,
            Dictionary<ResourceAddress, BuildingProceduralResource> current,
            ProceduralResourceBuild? previous
        ) {
            _session = session;
            _current = current;
            _previous = previous;
        }

        public ProceduralResourceBuild Build() {
            Debug.Assert(_current.Count > 0);

            ValidateProceduralResources();

            Dictionary<ResourceAddress, BuildingProceduralResource> next = [];

            // Assign the reference count to environment resources.
            foreach ((_, var proceduralResource) in _current) {
                foreach (var dependencyId in proceduralResource.DependencyAddresses) {
                    if (_session._graph.TryGetValue(dependencyId, out var environmentResourceVertex)) {
                        environmentResourceVertex.ReferenceCount++;
                    }
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
                    proceduralResource.Object.Dispose();
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
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }

                foreach (var dependencyID in _current[address].DependencyAddresses) {
                    if (!_current.ContainsKey(address)) continue;

                    Visit(dependencyID, temporaryMarks, permanentMarks, path);
                }

                permanentMarks.Add(address);
                path.Pop();
            }
        }

        private void BuildProceduralResource(
            ResourceAddress resourceAddress,
            BuildingProceduralResource resource,
            Dictionary<ResourceAddress, BuildingProceduralResource> next
        ) {
            if (_session.TryGetResult(resourceAddress, out _)) return;

            BuildEnvironment environment = _session._environment;

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
                    
                    ContentRepresentation processed;
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
                            processed.Dispose();
                        }
                    }
                    
                    _session.AppendProceduralResources(resourceAddress.LibraryId, processingContext.ProceduralResources, next);
                }
                
                _session.SetResult(resourceAddress, new(BuildStatus.Success));
                _session.AddOutputResourceRegistry(resourceAddress, new(null, resource.Tags));
            } finally {
                _session.ReleaseDependencies(resource.DependencyAddresses);
            }
        }
        
        private void CollectDependencies(
            IReadOnlySet<ResourceAddress> dependencyIds,
            out IReadOnlyDictionary<ResourceAddress, ContentRepresentation> collectedDependencies
        ) {
            if (dependencyIds.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceAddress, ContentRepresentation>.Empty;
                return;
            }
            
            Dictionary<ResourceAddress, ContentRepresentation> dependencyCollection = [];

            foreach (var dependencyAddress in dependencyIds) {
                if (_session._graph.TryGetValue(dependencyAddress, out EnvironmentResourceVertex? dependencyVertex)) {
                    if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                        dependencyCollection.Add(dependencyAddress, dependencyImportOutput);
                    } else {
                        if (_session.TryGetResult(dependencyAddress, out var result)) {
                            // Traversed through this resource vertex before.
                            switch (result.Status) {
                                case BuildStatus.Success:
                                    Debug.Assert(dependencyVertex.ImportOutput != null);
                        
                                    dependencyCollection[dependencyAddress] = dependencyVertex.ImportOutput;
                                    continue;
                    
                                case BuildStatus.Cached:
                                    // Resource is cached, but still import it to handle dependencies.
                                    // Fallthrough.
                                    break;
                    
                                default: continue;
                            }
                    
                            BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;

                            if (_session.Import(
                                dependencyAddress,
                                dependencyVertex.Library,
                                _session._environment.Importers[options.ImporterName],
                                options.Options,
                                out var imported,
                                out _)
                            ) {
                                dependencyVertex.SetImportResult(imported);
                                dependencyCollection.Add(dependencyAddress, imported);
                            }
                        }
                    }
                } else if (RecursivelyTryGetProceduralResource(dependencyAddress) is { } proceduralResource) {
                    dependencyCollection.Add(dependencyAddress, proceduralResource);
                }
            }

            collectedDependencies = dependencyCollection;
        }

        private ContentRepresentation? RecursivelyTryGetProceduralResource(ResourceAddress resourceAddress) {
            if (_current.TryGetValue(resourceAddress, out BuildingProceduralResource resource)) return resource.Object;
            
            return _previous?.RecursivelyTryGetProceduralResource(resourceAddress);
        }
    }
}
// ReSharper disable VariableHidesOuterVariable

using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildProceduralResources() {
        if (_proceduralResources.Count == 0) return;

        new ProceduralResourceBuild(this, _proceduralResources, null).Build();

        // while (_proceduralResources.Count > 0) {
        //     BuildCurrentLayer();
        //
        //     _proceduralResources.Clear();
        //     foreach ((var id, var resource) in _nextProceduralResources) {
        //         _proceduralResources.Add(id, resource);
        //     }
        //
        //     _nextProceduralResources.Clear();
        // }
        //
        // void BuildCurrentLayer() {
        //     
        // }
    }

    // public ProceduralBuildLayer Build() {
    //     if (_proceduralResources.Count == 0) return this;
    //     
    //     foreach ((ResourceID rid, BuildingProceduralResource resource) in _proceduralResources) {
    //         HashSet<ResourceID> sanitizedDependencies = new(resource.DependencyIds.Count);
    //
    //         foreach (var dependencyId in resource.DependencyIds) {
    //             if (_session._graph.ContainsKey(dependencyId) || _proceduralResources.ContainsKey(dependencyId)) {
    //                 if (dependencyId != rid) {
    //                     sanitizedDependencies.Add(dependencyId);
    //                 }
    //             }
    //         }
    //         
    //         ResourceVertex vertex = new(sanitizedDependencies);
    //         vertex.SetImportResult(resource.Object);
    //     
    //         _session._graph.Add(rid, vertex);
    //     }
    //
    //     try {
    //         _session.ValidateGraph();
    //
    //         // Not gonna do reference counting here because procedural resources are only created one, and it's very hard
    //         // to have an API that keeping track of refcount and releasing.
    //         
    //         foreach ((var rid, var vertex) in _proceduralResources) {
    //             BuildResource(rid, vertex, out _);
    //         }
    //
    //         return new ProceduralBuildLayer(_session, this, NextLayerProceduralResources).Build();
    //     } finally {
    //         foreach ((_, BuildingProceduralResource resource) in _proceduralResources) {
    //             resource.Object.Dispose();
    //         }
    //     }
    // }
    //
    // private void BuildResource(ResourceID rid, BuildingProceduralResource resource, out ResourceBuildingResult outputResult) {
    //     Debug.Assert(rid != ResourceID.Null);
    //     Debug.Assert(resource.Object != null);
    //
    //     var resourceVertex = _session._graph[rid];
    //     
    //     Debug.Assert(ReferenceEquals(resource.Object, resourceVertex.ImportOutput));
    //     
    //     try {
    //         if (string.IsNullOrEmpty(resource.ProcessorName)) {
    //             try {
    //                 _session.SerializeProcessedObject(resourceVertex.ImportOutput, rid, resource.Options, resource.Tags);
    //             } catch (Exception e) {
    //                 _session.Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
    //                 return;
    //             }
    //         } else if (_session._environment.Processors.TryGetValue(resource.ProcessorName, out var processor)) {
    //             if (!processor.CanProcess(resourceVertex.ImportOutput)) {
    //                 _session.Results.Add(rid, outputResult = new(BuildStatus.Unprocessable));
    //                 return;
    //             }
    //
    //             ContentRepresentation processed;
    //             IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies = CollectDependencies(resourceVertex.DependencyIds);
    //             ProcessingContext processingContext;
    //             
    //             try {
    //                 processingContext = new(_session._environment, rid, resource.Options, dependencies, _session._environment.Logger);
    //                 processed = processor.Process(resourceVertex.ImportOutput, processingContext);
    //             } catch (Exception e) {
    //                 _session.Results.Add(rid, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
    //                 return;
    //             }
    //
    //             try {
    //                 _session.SerializeProcessedObject(processed, rid, resource.Options, resource.Tags);
    //             } catch (Exception e) {
    //                 _session.Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
    //                 return;
    //             } finally {
    //                 if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
    //                     processed.Dispose();
    //                 }
    //             }
    //             
    //             _session.AppendProceduralResources(rid, processingContext.ProceduralResources, NextLayerProceduralResources);
    //         } else {
    //             _session.Results.Add(rid, outputResult = new(BuildStatus.UnknownProcessor));
    //             return;
    //         }
    //         
    //         _session.Results.Add(rid, outputResult = new(BuildStatus.Success));
    //     } finally {
    //         ReleaseDependencies(resource.DependencyIds);
    //     }
    //     
    //     IReadOnlyDictionary<ResourceID, ContentRepresentation> CollectDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
    //         if (dependencyIds.Count == 0) return FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
    //     
    //         Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];
    //
    //         foreach (var dependencyId in dependencyIds) {
    //             ResourceVertex dependencyVertex = _session._graph[dependencyId];
    //
    //             if (_session.Results.TryGetValue(dependencyId, out var result)) {
    //                 switch (result.Status) {
    //                     case BuildStatus.Success:
    //                         Debug.Assert(dependencyVertex.ImportOutput != null);
    //                     
    //                         dependencyCollection[dependencyId] = dependencyVertex.ImportOutput;
    //                         continue;
    //                 
    //                     case BuildStatus.Cached: break; // Import the resource.
    //                 
    //                     default: continue;
    //                 }
    //             }
    //
    //             if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
    //                 dependencyCollection.Add(dependencyId, dependencyImportOutput);
    //             } else {
    //                 if (_session._environment.Resources.TryGetValue(dependencyId, out BuildResourceRegistryElement registryElement)) {
    //                     (ResourceProvider provider, BuildingOptions options) = registryElement.Option;
    //                         
    //                     if (_session.Import(dependencyId, provider, _session._environment.Importers[options.ImporterName], options.Options, out ContentRepresentation? imported, out _)) {
    //                         dependencyVertex.SetImportResult(imported);
    //                         dependencyCollection.Add(dependencyId, imported);
    //                     }
    //                 } else if (_proceduralResources.TryGetValue(dependencyId, out var proceduralResource)) {
    //                     Debug.Assert(_session._graph[dependencyId].ReferenceCount > 0);
    //                     
    //                     dependencyCollection.Add(dependencyId, proceduralResource.Object);
    //                 }
    //             }
    //         }
    //
    //         return dependencyCollection;
    //     }
    // }
    //
    // private void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds) {
    //     foreach (var dependencyId in dependencyIds) {
    //         if (!_session._environment.Resources.ContainsKey(dependencyId)) continue;   // Only release environment resources.
    //         
    //         _session._graph[dependencyId].Release();
    //     }
    // }

    private sealed class ProceduralResourceBuild {
        private readonly BuildSession _session;
        private readonly Dictionary<ResourceID, BuildingProceduralResource> _current;
        private readonly ProceduralResourceBuild? _previous;

        public ProceduralResourceBuild(BuildSession session, Dictionary<ResourceID, BuildingProceduralResource> current, ProceduralResourceBuild? previous) {
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

        private void BuildProceduralResource(ResourceID resourceId, BuildingProceduralResource resource, Dictionary<ResourceID, BuildingProceduralResource> next) {
            Debug.Assert(resourceId != ResourceID.Null);
            
            if (_session.Results.ContainsKey(resourceId)) return;

            BuildEnvironment environment = _session._environment;

            // CollectDependencies(resource.DependencyIds, out var collectedDependencies, out var validDependencyIds);

            try {
                string? processorName = resource.ProcessorName;

                IReadOnlySet<ResourceID> validDependencyIds;
                
                if (string.IsNullOrEmpty(processorName)) {
                    try {
                        _session.SerializeProcessedObject(resource.Object, resourceId, resource.Options, resource.Tags);
                    } catch (Exception e) {
                        _session.Results.Add(resourceId, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                        return;
                    }
                    
                    validDependencyIds = FrozenSet<ResourceID>.Empty;
                } else {
                    if (!environment.Processors.TryGetValue(processorName, out var processor)) {
                        _session.Results.Add(resourceId, new(BuildStatus.UnknownProcessor));
                        return;
                    }
                    
                    if (!processor.CanProcess(resource.Object)) {
                        _session.Results.Add(resourceId, new(BuildStatus.Unprocessable));
                        return;
                    }
                    
                    CollectDependencies(resource.DependencyIds, out var dependencies, out validDependencyIds);
                    
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

            return;

            void CollectDependencies(IReadOnlySet<ResourceID> dependencyIds, out IReadOnlyDictionary<ResourceID, ContentRepresentation> collectedDependencies, out IReadOnlySet<ResourceID> validDependencies) {
                if (dependencyIds.Count == 0) {
                    collectedDependencies = FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
                    validDependencies = FrozenSet<ResourceID>.Empty;
                    return;
                }
                
                Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];
                HashSet<ResourceID> validDependencyIds = [];

                foreach (var dependencyId in dependencyIds) {
                    if (_session._graph.TryGetValue(dependencyId, out EnvironmentResourceVertex? dependencyVertex)) {
                        if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                            dependencyCollection.Add(dependencyId, dependencyImportOutput);
                            validDependencyIds.Add(dependencyId);
                        } else {
                            if (_session.Results.TryGetValue(dependencyId, out var result)) {
                                // Traversed through this resource vertex before.
                                switch (result.Status) {
                                    case BuildStatus.Success:
                                        Debug.Assert(dependencyVertex.ImportOutput != null);
                            
                                        dependencyCollection[dependencyId] = dependencyVertex.ImportOutput;
                                        validDependencyIds.Add(dependencyId);
                                        continue;
                        
                                    case BuildStatus.Cached:
                                        // Resource is cached, but still import it to handle dependencies.
                                        // Fallthrough.
                                        break;
                        
                                    default: continue;
                                }
                        
                                BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;
                        
                                if (_session.Import(dependencyId, dependencyVertex.Library, _session._environment.Importers[options.ImporterName], options.Options, out ContentRepresentation? imported, out _)) {
                                    dependencyVertex.SetImportResult(imported);
                                    dependencyCollection.Add(dependencyId, imported);
                                    validDependencyIds.Add(dependencyId);
                                }
                            }
                        }
                    } else if (TryGetProceduralResourceRecursively(dependencyId) is { } proceduralResource) {
                        dependencyCollection.Add(dependencyId, proceduralResource);
                        validDependencyIds.Add(dependencyId);
                    }
                }

                collectedDependencies = dependencyCollection;
                validDependencies = validDependencyIds;
            }
        }

        private ContentRepresentation? TryGetProceduralResourceRecursively(ResourceID resourceId) {
            if (_current.TryGetValue(resourceId, out BuildingProceduralResource resource)) return resource.Object;
            
            return _previous?.TryGetProceduralResourceRecursively(resourceId);
        }
    }
}
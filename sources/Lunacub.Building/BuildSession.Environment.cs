using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
// ReSharper disable VariableHidesOuterVariable

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources() {
        _environment.Logger.LogInformation("Building environment resources.");
        
        foreach (var library in _environment.Libraries) {
            foreach ((var rid, var element) in library.Registry) {
                var option = element.Option;

                if (!_environment.Importers.TryGetValue(option.Options.ImporterName, out var importer)) {
                    Results.Add(rid, new(BuildStatus.UnknownImporter));
                    continue;
                }

                // TODO: Add an optimization flag in Importer to skip this part.
                HashSet<ResourceID> dependencyIds;

                if (library.CreateResourceStream(rid) is not { } resourceStream) {
                    Results.Add(rid, new(BuildStatus.NullResourceStream));
                    continue;
                }

                try {
                    IReadOnlyCollection<ResourceID> extractedDependencies = importer.ExtractDependencies(resourceStream);
                    dependencyIds = extractedDependencies.ToHashSet();
                    
                    dependencyIds.Remove(rid); // Preventing self-referencing break everything.
                } catch (Exception e) {
                    Results.Add(rid, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                    continue;
                } finally {
                    resourceStream.Dispose();
                }

                _graph.Add(rid, new(library, dependencyIds, element));
            }
        }

        // Validate dependency graph.
        ValidateGraph();
        
        // Assigning reference count for dependencies.
        foreach ((_, var vertex) in _graph) {
            foreach (var dependencyId in vertex.DependencyIds) {
                if (!_graph.TryGetValue(dependencyId, out EnvironmentResourceVertex? dependencyVertex)) continue;
                
                dependencyVertex.ReferenceCount++;
            }

            vertex.ReferenceCount++;    // Environment resources need the import output to process and serialize.
        }

        // Handle the building procedure.
        foreach ((var rid, var vertex) in _graph) {
            BuildEnvironmentResource(rid, vertex, out _);
        }
        
        Debug.Assert(_graph.Values.All(x => x.ReferenceCount == 0));
        
        void ValidateGraph() {
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
    }
    
    private void BuildEnvironmentResource(ResourceID rid, EnvironmentResourceVertex resourceVertex, out ResourceBuildingResult outputResult) {
        Debug.Assert(rid != ResourceID.Null);
        
        if (Results.TryGetValue(rid, out outputResult)) return;
        
        bool dependencyRebuilt = false;
        foreach (var dependencyId in resourceVertex.DependencyIds) {
            if (!_graph.TryGetValue(dependencyId, out var dependencyVertexInfo)) continue;
            
            BuildEnvironmentResource(dependencyId, dependencyVertexInfo, out ResourceBuildingResult dependencyResult);

            if (dependencyResult.Status == BuildStatus.Success) {
                dependencyRebuilt = true;
            }
        }

        try {
            ResourceRegistry<BuildingResource>.Element registryElement = resourceVertex.RegistryElement;
            BuildingOptions options = registryElement.Option.Options;

            Importer importer = _environment.Importers[options.ImporterName];

            Processor? processor = null;

            if (!string.IsNullOrEmpty(options.ProcessorName) && !_environment.Processors.TryGetValue(options.ProcessorName, out processor)) {
                Results.Add(rid, outputResult = new(BuildStatus.UnknownProcessor));
                return;
            }

            DateTime resourceLastWriteTime = resourceVertex.Library.GetResourceLastWriteTime(rid);
            
            if (!dependencyRebuilt && IsResourceCacheable(rid, resourceLastWriteTime, options, resourceVertex.DependencyIds, out IncrementalInfo previousIncrementalInfo)) {
                if (new ComponentVersions(importer.Version, processor?.Version) == previousIncrementalInfo.ComponentVersions) {
                    Results.Add(rid, outputResult = new(BuildStatus.Cached));
                    _outputRegistry.Add(rid, new(registryElement.Name, registryElement.Tags));
                    return;
                }
            }
            
            // Import if haven't.
            if (resourceVertex.ImportOutput == null) {
                if (!Import(rid, resourceVertex.Library, importer, options.Options, out ContentRepresentation? importOutput, out outputResult)) {
                    return;
                }
                
                resourceVertex.SetImportResult(importOutput);
            }

            IReadOnlySet<ResourceID> dependencyIds;
            
            if (processor == null) {
                try {
                    SerializeProcessedObject(resourceVertex.ImportOutput, rid, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
                
                dependencyIds = FrozenSet<ResourceID>.Empty;
            } else {
                if (!processor.CanProcess(resourceVertex.ImportOutput)) {
                    Results.Add(rid, outputResult = new(BuildStatus.Unprocessable));
                    return;
                }

                CollectDependencies(resourceVertex.DependencyIds, out IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies, out IReadOnlySet<ResourceID> validDependencyIds);
                resourceVertex.DependencyIds = validDependencyIds;

                ContentRepresentation processed;
                ProcessingContext processingContext;
                
                try {
                    processingContext = new(_environment, rid, options.Options, dependencies, _environment.Logger);
                    processed = processor.Process(resourceVertex.ImportOutput, processingContext);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }

                try {
                    SerializeProcessedObject(processed, rid, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    Results.Add(rid, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
                        processed.Dispose();
                    }
                }
                
                AppendProceduralResources(rid, processingContext.ProceduralResources, _proceduralResources);
                dependencyIds = validDependencyIds;
            }

            Results.Add(rid, outputResult = new(BuildStatus.Success));
            _environment.IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, dependencyIds, new(importer.Version, processor?.Version)));
            _outputRegistry.Add(rid, new(registryElement.Name, registryElement.Tags));
        } finally {
            ReleaseDependencies(resourceVertex.DependencyIds);
            resourceVertex.Release();
        }

        return;

        void CollectDependencies(IReadOnlyCollection<ResourceID> dependencyIds, out IReadOnlyDictionary<ResourceID, ContentRepresentation> collectedDependencies, out IReadOnlySet<ResourceID> validDependencies) {
            if (dependencyIds.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceID, ContentRepresentation>.Empty;
                validDependencies = FrozenSet<ResourceID>.Empty;
                return;
            }
        
            Dictionary<ResourceID, ContentRepresentation> dependencyCollection = [];
            HashSet<ResourceID> validDependencyIds = [];

            foreach (var dependencyId in dependencyIds) {
                if (!_graph.TryGetValue(dependencyId, out EnvironmentResourceVertex? dependencyVertex)) continue;

                if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                    dependencyCollection.Add(dependencyId, dependencyImportOutput);
                    validDependencyIds.Add(dependencyId);
                } else {
                    if (Results.TryGetValue(dependencyId, out var result)) {
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
                        
                        if (Import(dependencyId, dependencyVertex.Library, _environment.Importers[options.ImporterName], options.Options, out ContentRepresentation? imported, out _)) {
                            dependencyVertex.SetImportResult(imported);
                            dependencyCollection.Add(dependencyId, imported);
                            validDependencyIds.Add(dependencyId);
                        }
                    }
                }
            }

            collectedDependencies = dependencyCollection;
            validDependencies = validDependencyIds;
        }
    }
}
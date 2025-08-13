// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources() {
        // TODO: Parallel this foreach
        foreach ((LibraryID libraryId, (BuildResourceLibrary library, Dictionary<ResourceID, EnvironmentResourceVertex> vertices)) in _graph) {
            foreach ((var resourceId, var element) in library.Registry) {
                BuildingResource resource = element.Option;
                
                if (!_environment.Importers.TryGetValue(resource.Options.ImporterName, out var importer)) {
                    SetResult(libraryId, resourceId, new(BuildStatus.UnknownImporter));
                    continue;
                }

                try {
                    importer.ValidateResource(resource);
                } catch (Exception e) {
                    SetResult(libraryId, resourceId, new(BuildStatus.InvalidBuildingResource, ExceptionDispatchInfo.Capture(e)));
                    continue;
                }

                IReadOnlySet<ResourceAddress> dependencyAddresses;
                
                if (importer.Flags.HasFlag(ImporterFlags.NoDependency)) {
                    dependencyAddresses = FrozenSet<ResourceAddress>.Empty;
                } else {
                    using SourceStreams streams = library.CreateSourceStreams(resourceId);
                    
                    if (streams.PrimaryStream == null) {
                        SetResult(libraryId, resourceId, new(BuildStatus.NullPrimaryResourceStream));
                        continue;
                    }

                    foreach ((_, var secondaryStream) in streams.SecondaryStreams) {
                        if (secondaryStream == null) {
                            SetResult(libraryId, resourceId, new(BuildStatus.NullSecondaryResourceStream));
                            break;
                        }
                    }

                    if (TryGetResult(libraryId, resourceId, out _)) continue;
                    
                    try {
                        IReadOnlyCollection<ResourceAddress> extractedDependencies = importer.ExtractDependencies(streams);

                        if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(new(libraryId, resourceId))) {
                            dependencyAddresses = dependencySet;
                        } else {
                            HashSet<ResourceAddress> createdSet = new(extractedDependencies);
                            createdSet.Remove(new(libraryId, resourceId));
                            dependencyAddresses = createdSet;
                        }
                    } catch (Exception e) {
                        SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                        continue;
                    }
                }

                vertices.Add(resourceId, new(importer, dependencyAddresses));
            }
        }

        // Validate dependency graph.
        ValidateGraph();
        
        // Assigning reference count for dependencies.
        Parallel.ForEach(_graph, (kvp, state, index) => {
            var libraryVertices = kvp.Value;
            
            foreach ((_, var vertex) in libraryVertices.Vertices) {
                foreach (var dependencyAddress in vertex.DependencyResourceAddresses) {
                    if (!TryGetVertex(dependencyAddress, out var dependencyVertex)) continue;
                    
                    dependencyVertex.IncrementReference();
                }
                
                vertex.IncrementReference();
            }
        });

        // Handle the building procedure.
        foreach ((var libraryId, (var library, var libraryVertices)) in _graph) {
            foreach ((var resourceId, var resourceVertex) in libraryVertices) {
                BuildEnvironmentResource(new(libraryId, resourceId), library, resourceVertex, out _);
            }
        }
        
        Debug.Assert(_graph.Values.SelectMany(x => x.Vertices.Values).All(x => x.ReferenceCount == 0));
        return;
        
        void ValidateGraph() {
            if (_graph.Count == 0) return;
        
            HashSet<ResourceAddress> temporaryMarks = [], permanentMarks = [];
            Stack<ResourceAddress> path = [];
            
            foreach ((var libraryId, var libraryVertices) in _graph) {
                foreach ((var resourceId, var resourceVertex) in libraryVertices.Vertices) {
                    Visit(new(libraryId, resourceId), resourceVertex, temporaryMarks, permanentMarks, path);
                }
            }
            
            return;

            void Visit(
                ResourceAddress resourceAddress,
                EnvironmentResourceVertex resourceVertex,
                HashSet<ResourceAddress> temporaryMarks,
                HashSet<ResourceAddress> permanentMarks,
                Stack<ResourceAddress> path
            ) {
                if (permanentMarks.Contains(resourceAddress)) return;
                
                path.Push(resourceAddress);
                
                if (!temporaryMarks.Add(resourceAddress)) {
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }
                
                foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses) {
                    if (!TryGetVertex(dependencyAddress, out var dependencyVertex)) continue;
                    
                    Visit(dependencyAddress, dependencyVertex, temporaryMarks, permanentMarks, path);
                }
                
                permanentMarks.Add(resourceAddress);
                path.Pop();
            }
        }
    }
    
    private void BuildEnvironmentResource(
        ResourceAddress resourceAddress,
        BuildResourceLibrary resourceLibrary,
        EnvironmentResourceVertex resourceVertex,
        out ResourceBuildingResult outputResult
    ) {
        if (TryGetResult(resourceAddress, out outputResult)) return;
        
        Debug.Assert(resourceAddress.LibraryId == resourceLibrary.Id);
        
        bool dependencyRebuilt = false;
        foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses) {
            if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;
            
            BuildEnvironmentResource(dependencyAddress, dependencyResourceLibrary, dependencyVertex, out ResourceBuildingResult dependencyResult);

            if (dependencyResult.Status == BuildStatus.Success) {
                dependencyRebuilt = true;
            }
        }

        try {
            // ResourceRegistry.Element<BuildingResource> registryElement = resourceVertex.RegistryElement;
            ResourceRegistry.Element<BuildingResource> registryElement = resourceLibrary.Registry[resourceAddress.ResourceId];
            BuildingOptions options = registryElement.Option.Options;

            string? processorName = options.ProcessorName;
            Processor? processor = null;

            if (!string.IsNullOrEmpty(processorName) && !_environment.Processors.TryGetValue(processorName, out processor)) {
                SetResult(resourceAddress, outputResult = new(BuildStatus.UnknownProcessor));
                return;
            }

            SourceLastWriteTimes lastWriteTimes;

            try {
                lastWriteTimes = resourceLibrary.GetSourceLastWriteTimes(resourceAddress.ResourceId);
            } catch (Exception e) {
                SetResult(resourceAddress, outputResult = new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }

            var importer = resourceVertex.Importer;
            
            if (!dependencyRebuilt) {
                if (IsResourceCacheable(resourceAddress, lastWriteTimes, options, resourceVertex.DependencyResourceAddresses, out var previousIncrementalInfo)) {
                    if (new ComponentVersions(importer.Version, processor?.Version) == previousIncrementalInfo.ComponentVersions) {
                        SetResult(resourceAddress, outputResult = new(BuildStatus.Cached));
                        AddOutputResourceRegistry(resourceAddress, new(registryElement.Name, registryElement.Tags));
                        return;
                    }
                }
            }
            
            // Import if haven't.
            if (resourceVertex.ImportOutput == null) {
                if (!Import(resourceAddress, resourceLibrary, importer, options.Options, out var importOutput, out outputResult)) {
                    return;
                }
                
                resourceVertex.SetImportResult(importOutput);
            }

            IReadOnlySet<ResourceAddress> dependencyIds;
            
            if (processor == null) {
                try {
                    SerializeProcessedObject(resourceVertex.ImportOutput, resourceAddress, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(resourceAddress, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
                
                dependencyIds = FrozenSet<ResourceAddress>.Empty;
            } else {
                if (!processor.CanProcess(resourceVertex.ImportOutput)) {
                    SetResult(resourceAddress, outputResult = new(BuildStatus.Unprocessable));
                    return;
                }

                CollectDependencies(
                    resourceVertex.DependencyResourceAddresses,
                    out IReadOnlyDictionary<ResourceAddress, object> dependencies,
                    out IReadOnlySet<ResourceAddress> validDependencyIds
                );
                resourceVertex.DependencyResourceAddresses = validDependencyIds;

                object processed;
                ProcessingContext processingContext;
                
                try {
                    processingContext = new(_environment, resourceAddress, options.Options, dependencies, _environment.Logger);
                    processed = processor.Process(resourceVertex.ImportOutput, processingContext);
                } catch (Exception e) {
                    SetResult(resourceAddress, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }

                try {
                    SerializeProcessedObject(processed, resourceAddress, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(resourceAddress, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
                        processor.Dispose(processed, new(_environment.Logger));
                    }
                }
                
                AppendProceduralResources(resourceAddress, processingContext.ProceduralResources, _proceduralResources);
                dependencyIds = validDependencyIds;
            }

            SetResult(resourceAddress, outputResult = new(BuildStatus.Success));
            _environment.IncrementalInfos.SetIncrementalInfo(resourceAddress, new(lastWriteTimes, options, dependencyIds, new(importer.Version, processor?.Version)));
            AddOutputResourceRegistry(resourceAddress, new(registryElement.Name, registryElement.Tags));
        } finally {
            ReleaseDependencies(resourceVertex.DependencyResourceAddresses);
            ReleaseVertexOutput(resourceVertex);
        }

        return;

        void CollectDependencies(
            IReadOnlyCollection<ResourceAddress> dependencyAddresses,
            out IReadOnlyDictionary<ResourceAddress, object> collectedDependencies,
            out IReadOnlySet<ResourceAddress> validDependencies
        ) {
            if (dependencyAddresses.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceAddress, object>.Empty;
                validDependencies = FrozenSet<ResourceAddress>.Empty;
                return;
            }
        
            Dictionary<ResourceAddress, object> dependencyCollection = [];
            HashSet<ResourceAddress> validDependencyIds = [];

            foreach (var dependencyAddress in dependencyAddresses) {
                if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;

                if (dependencyVertex.ImportOutput is { } dependencyImportOutput) {
                    dependencyCollection.Add(dependencyAddress, dependencyImportOutput);
                    validDependencyIds.Add(dependencyAddress);
                } else {
                    if (TryGetResult(dependencyAddress, out var result)) {
                        // Traversed through this resource vertex before.
                        switch (result.Status) {
                            case BuildStatus.Success:
                                Debug.Assert(dependencyVertex.ImportOutput != null);
                            
                                dependencyCollection[dependencyAddress] = dependencyVertex.ImportOutput;
                                validDependencyIds.Add(dependencyAddress);
                                continue;
                        
                            case BuildStatus.Cached:
                                // Resource is cached, but still import it to handle dependencies.
                                // Fallthrough.
                                break;
                        
                            default: continue;
                        }
                        
                        // BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;
                        BuildingOptions options = dependencyResourceLibrary.Registry[dependencyAddress.ResourceId].Option.Options;

                        Importer importer = dependencyVertex.Importer;
                        if (Import(dependencyAddress, dependencyResourceLibrary, importer, options.Options, out var imported, out _)) {
                            dependencyVertex.SetImportResult(imported);
                            dependencyCollection.Add(dependencyAddress, imported);
                            validDependencyIds.Add(dependencyAddress);
                        }
                    }
                }
            }

            collectedDependencies = dependencyCollection;
            validDependencies = validDependencyIds;
        }
    }
    
    private bool Import(
        ResourceAddress address,
        BuildResourceLibrary library,
        Importer importer,
        IImportOptions? options,
        [NotNullWhen(true)] out object? imported,
        out ResourceBuildingResult failureResult
    ) {
        SourceStreams streams;

        try {
            streams = library.CreateSourceStreams(address.ResourceId);
        } catch (InvalidResourceStreamException e) {
            SetResult(address, failureResult = new(BuildStatus.InvalidResourceStream, ExceptionDispatchInfo.Capture(e)));
            imported = null;
            return false;
        }

        try {
            if (streams.PrimaryStream is null) {
                SetResult(address, failureResult = new(BuildStatus.NullPrimaryResourceStream));
                imported = null;
                return false;
            }
            
            try {
                ImportingContext context = new(options, _environment.Logger);
                imported = importer.ImportObject(streams, context);
                
                failureResult = default;
                return true;
            } catch (Exception e) {
                SetResult(address, failureResult = new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                imported = null;
                return false;
            }
        } finally {
            streams.Dispose();
        }
    }
}
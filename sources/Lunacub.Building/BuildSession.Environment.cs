// ReSharper disable VariableHidesOuterVariable
namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources(bool rebuild) {
        Log.BeginBuildingEnvironmentResources(_environment.Logger);
        
        // Populate the graph vertices.
        CreateGraphVertices(rebuild);
        
        if (Results.Count > 0) return;

        bool cycleDetected = false;
        CheckGraphCycle(_ => {
            Log.WarnGraphCycleDetectedAfterPopulateVerticesFromIncrementalInfos(_environment.Logger);
            cycleDetected = true;
        });

        // Extract all dependencies.
        if (cycleDetected) {
            ExtractAllVerticesDependencies();
        } else {
            ExtractChangedVerticesDependencies();
        }

        if (Results.Count > 0) return;
        
        CheckGraphCycle(path => {
            string message = string.Format(ExceptionMessages.CircularResourceDependency, string.Join(" -> ", path.Reverse()));
            throw new InvalidOperationException(message);
        });
        
        // Assign reference count.
        CountReferences();
        
        // Shouldn't cause any error.
        Debug.Assert(Results.Count == 0);

        try {
            // Alright everything has been populated, begin building resources.
            InnerBuildEnvironmentResources();
        } finally {
            bool reported = false;

            foreach ((_, var libraryVertices) in _graph) {
                foreach ((_, var vertex) in libraryVertices.Vertices) {
                    if (vertex.ObjectRepresentation != null) {
                        if (!reported) {
                            reported = true;
                            Log.ReportLeakedAfterBuildEnvironmentResources(_environment.Logger);
                        }
                        
                        vertex.DisposeImportedObject(new(_environment.Logger));
                    } else if (vertex.ReferenceCount > 0) {
                        if (!reported) {
                            reported = true;
                            Log.ReportLeakedAfterBuildEnvironmentResources(_environment.Logger);
                        }
                    }
                }
                
                // We don't need these vertices anymore because we've finish building the environment resources.
                // Await more vertices at procedural resources building stages.
                libraryVertices.Vertices.Clear();
            }
        }
        
        // We're done building environment resources, begin building procedural resources.
        BuildProceduralResources();
        return;
    }

    /// <summary>
    /// Populate graph vertices from environment resources.
    /// </summary>
    /// <remarks>
    /// Unchanged resources will have its dependency collection populated form the previous incremental infos.
    /// Changed resources will have its dependency collection null on purpose.
    /// </remarks>
    private void CreateGraphVertices(bool rebuild) {
        Log.CreateVertices(_environment.Logger);
        
        Parallel.ForEach(_graph, (graphKvp, _, _) => {
            (var libraryId, (var library, var vertexDictionary)) = graphKvp;
        
            foreach ((ResourceID resourceId, ResourceRegistry.Element<BuildingResource> element) in library.Registry) {
                BuildingResource resource = element.Option;
        
                Processor? processor = null;
        
                string importerName = resource.Options.ImporterName;
                string? processorName = resource.Options.ProcessorName;
        
                // Get the associating Importer and Processor.
                if (!_environment.Importers.TryGetValue(importerName, out Importer? importer)) {
                    SetResult(libraryId, resourceId, new(BuildStatus.UnknownImporter));
                    continue;
                }
                
                // Validate resource.
                try {
                    importer.ValidateResource(resource);
                } catch (Exception e) {
                    SetResult(libraryId, resourceId, new(BuildStatus.InvalidBuildingResource, ExceptionDispatchInfo.Capture(e)));
                    continue;
                }
                
                if (!string.IsNullOrEmpty(processorName) && !_environment.Processors.TryGetValue(processorName, out processor)) {
                    SetResult(libraryId, resourceId, new(BuildStatus.UnknownProcessor));
                    continue;
                }
                
                // Get the source informations, we always need this information, whether or not the resource is changed or not.
                SourcesInfo sourcesInformation;
        
                try {
                    sourcesInformation = library.GetSourcesInformations(resourceId);
                } catch (Exception e) {
                    SetResult(libraryId, resourceId, new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
                    continue;
                }
                
                if (!rebuild && _environment.BuildCache.TryGetValue(libraryId, out LibraryBuildCache? libraryIncrementalInfos)) {
                    if (libraryIncrementalInfos.TryGetValue(resourceId, out BuildCache previousIncrementalInfo)) {
                        // So we do got the report from the last building session.
                        
                        // Compare the components to early opt-out as soon as possible.
                        if (previousIncrementalInfo.Options.ImporterName == importerName && previousIncrementalInfo.Options.ProcessorName == processorName) {
                            if (previousIncrementalInfo.ComponentVersions.ImporterVersion == importer.Version && (processor == null || previousIncrementalInfo.ComponentVersions.ProcessorVersion == processor.Version)) {
                                // Welp, parameter seems fine, check the sources information to see if it got any changes.
        
                                // Check the sources integrity
                                if (CheckSourcesIntegrity(previousIncrementalInfo.SourcesInfo, sourcesInformation)) {
                                    vertexDictionary.Add(resourceId, new(importer, processor, sourcesInformation) {
                                        DependencyResourceAddresses = previousIncrementalInfo.DependencyAddresses,
                                        IsSelfUnchanged = true,
                                    });
                                    continue;
                                }
                            }
                        }
                    }
                }
                
                // Set the dependency collection null on purpose.
                vertexDictionary.Add(resourceId, new(importer, processor, sourcesInformation) {
                    DependencyResourceAddresses = null,
                    IsSelfUnchanged = false,
                });
            }
        });

        return;
        
        // Check the sources integrity:
        // 1. If the addresses are the same as the addresses in the last building session.
        // 2. If those sources has no change (by comparing the last write time).
        static bool CheckSourcesIntegrity(SourcesInfo previous, SourcesInfo current) {
            if (!CheckSourceIntegrity(previous.Primary, current.Primary)) return false;
        
            IReadOnlyDictionary<string, SourceInfo> previousSecondaries = previous.Secondaries ?? FrozenDictionary<string, SourceInfo>.Empty;
            IReadOnlyDictionary<string, SourceInfo> currentSecondaries = current.Secondaries ?? FrozenDictionary<string, SourceInfo>.Empty;
        
            if (previousSecondaries.Count != currentSecondaries.Count) return false;
        
            foreach ((var previousKey, var previousSourceInfo) in previousSecondaries) {
                if (!currentSecondaries.TryGetValue(previousKey, out var currentSourceInfo)) return false;
                if (!CheckSourceIntegrity(previousSourceInfo, currentSourceInfo)) return false;
            }
        
            return true;
        }
        
        static bool CheckSourceIntegrity(SourceInfo previous, SourceInfo current) {
            if (current.Address != previous.Address) return false;
            if (current.LastWriteTime > previous.LastWriteTime) return false;
        
            // TODO: Check content checksum?
            return true;
        }
    }

    private void ExtractChangedVerticesDependencies() {
        Log.ExtractVerticesDependenciesForModified(_environment.Logger);
        
        Parallel.ForEach(_graph, (graphKvp, _, _) => {
            (var libraryId, (var library, var vertexDictionary)) = graphKvp;

            foreach ((ResourceID resourceId, _) in library.Registry) {
                ResourceAddress address = new(libraryId, resourceId);
                
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex != null);
                
                if (vertex.IsSelfUnchanged) continue;

                if (vertex.Importer.Flags.HasFlag(ImporterFlags.NoDependency)) {
                    vertex.DependencyResourceAddresses = FrozenSet<ResourceAddress>.Empty;
                    continue;
                }
                
                Debug.Assert(vertex.DependencyResourceAddresses is null);
                
                try {
                    using SourceStreams streams = library.CreateSourceStreams(resourceId);
                    
                    try {
                        IReadOnlySet<ResourceAddress> dependencies;
                        IReadOnlyCollection<ResourceAddress> extractedDependencies = vertex.Importer.ExtractDependencies(streams);
                
                        if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(address)) {
                            dependencies = dependencySet;
                        } else {
                            HashSet<ResourceAddress> createdSet = new(extractedDependencies);
                            createdSet.Remove(address);
                            dependencies = createdSet;
                        }

                        vertex.DependencyResourceAddresses = dependencies;
                    } catch (Exception e) {
                        SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                    }
                } catch (Exception e) {
                    SetResult(libraryId, resourceId, new(BuildStatus.GetSourceStreamsFail, ExceptionDispatchInfo.Capture(e)));
                }
            }
        });
    }

    private void ExtractAllVerticesDependencies() {
        Log.ExtractVerticesDependenciesForAll(_environment.Logger);
        
        Parallel.ForEach(_graph, (graphKvp, _, _) => {
            (var libraryId, (var library, var vertexDictionary)) = graphKvp;

            foreach ((ResourceID resourceId, _) in library.Registry) {
                ResourceAddress address = new(libraryId, resourceId);
                
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex != null);

                vertex.IsSelfUnchanged = false;

                if (vertex.Importer.Flags.HasFlag(ImporterFlags.NoDependency)) {
                    vertex.DependencyResourceAddresses = FrozenSet<ResourceAddress>.Empty;
                    continue;
                }
                
                Debug.Assert(vertex.DependencyResourceAddresses is null);
                
                try {
                    using SourceStreams streams = library.CreateSourceStreams(resourceId);
                    
                    try {
                        IReadOnlySet<ResourceAddress> dependencies;
                        IReadOnlyCollection<ResourceAddress> extractedDependencies = vertex.Importer.ExtractDependencies(streams);
                
                        if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(address)) {
                            dependencies = dependencySet;
                        } else {
                            HashSet<ResourceAddress> createdSet = new(extractedDependencies);
                            createdSet.Remove(address);
                            dependencies = createdSet;
                        }

                        vertex.DependencyResourceAddresses = dependencies;
                    } catch (Exception e) {
                        SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                    }
                } catch (Exception e) {
                    SetResult(libraryId, resourceId, new(BuildStatus.GetSourceStreamsFail, ExceptionDispatchInfo.Capture(e)));
                }
            }
        });
    }

    private void CountReferences() {
        Log.CountResourceVertexReferences(_environment.Logger);
        
        Parallel.ForEach(_graph, (graphKvp, _, _) => {
            (_, (var library, var vertexDictionary)) = graphKvp;

            foreach ((ResourceID resourceId, _) in library.Registry) {
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex != null);
                Debug.Assert(vertex.DependencyResourceAddresses != null);

                foreach (var dependencyAddress in vertex.DependencyResourceAddresses) {
                    if (!TryGetVertex(dependencyAddress, out _, out var dependencyVertex)) continue;
                    
                    dependencyVertex.IncrementReference();
                }
                
                // Self-reference.
                vertex.IncrementReference();
            }
        });
    }

    private void InnerBuildEnvironmentResources() {
        Log.CompileResources(_environment.Logger);
        
        foreach ((var libraryId, (BuildResourceLibrary library, var vertexDictionary)) in _graph) {
            foreach ((var resourceId, _) in library.Registry) {
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex is not null);
                
                BuildSingleResource(new(libraryId, resourceId), library, vertex);
            }
        }
    }

    private void BuildSingleResource(ResourceAddress resourceAddress, BuildResourceLibrary resourceLibrary, ResourceVertex resourceVertex) {
        try {
            if (resourceVertex.IsBuilt) return;
            resourceVertex.IsBuilt = true;
            
            Debug.Assert(resourceAddress.LibraryId == resourceLibrary.Id);
        
            bool dependencyRebuilt = false;
            foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses ?? FrozenSet<ResourceAddress>.Empty) {
                if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;
                
                BuildSingleResource(dependencyAddress, dependencyResourceLibrary, dependencyVertex);
            
                if (!dependencyVertex.IsSelfUnchanged) {
                    dependencyRebuilt = true;
                }
            }

            ResourceRegistry.Element<BuildingResource> registryElement = resourceLibrary.Registry[resourceAddress.ResourceId];
            BuildingOptions options = registryElement.Option.Options;

            var importer = resourceVertex.Importer;

            if (!dependencyRebuilt && resourceVertex.IsSelfUnchanged) return;
            
            // Import if haven't.
            if (resourceVertex.ObjectRepresentation == null) {
                if (!Import(resourceAddress, resourceLibrary, importer, options.Options, out var importOutput, out _)) {
                    return;
                }
                
                resourceVertex.SetObjectRepresentation(importOutput);
            }
            
            IReadOnlySet<ResourceAddress> dependencyIds;
            
            if (resourceVertex.Processor is not { } processor) {
                try {
                    SerializeProcessedObject(resourceVertex.ObjectRepresentation, resourceAddress, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
                
                dependencyIds = FrozenSet<ResourceAddress>.Empty;
            } else {
                if (!processor.CanProcess(resourceVertex.ObjectRepresentation)) {
                    SetResult(resourceAddress, new(BuildStatus.Unprocessable));
                    return;
                }
            
                CollectDependencies(
                    resourceVertex.DependencyResourceAddresses!,
                    out IReadOnlyDictionary<ResourceAddress, object> dependencies,
                    out IReadOnlySet<ResourceAddress> validDependencyIds
                );
                resourceVertex.DependencyResourceAddresses = validDependencyIds;
            
                object processed;
                ProcessingContext processingContext;
                
                try {
                    processingContext = new(_environment, resourceAddress, options.Options, dependencies, _environment.Logger);
                    processed = processor.Process(resourceVertex.ObjectRepresentation, processingContext);
                } catch (Exception e) {
                    SetResult(resourceAddress, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
            
                try {
                    SerializeProcessedObject(processed, resourceAddress, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(resourceAddress, new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    if (!ReferenceEquals(resourceVertex.ObjectRepresentation, processed)) {
                        processor.Dispose(processed, new(_environment.Logger));
                    }
                }
                
                AppendProceduralResources(resourceAddress, processingContext.ProceduralResources, _proceduralResources);
                dependencyIds = validDependencyIds;
            }
            
            _environment.BuildCache.SetIncrementalInfo(resourceAddress, new(resourceVertex.SourcesInformation, options, dependencyIds, new(importer.Version, resourceVertex.Processor?.Version)));
            AddOutputResourceRegistry(resourceAddress, new(registryElement.Name, registryElement.Tags));
        } finally {
            ReleaseVertex(resourceVertex);
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
        
                if (dependencyVertex.ObjectRepresentation is { } dependencyImportOutput) {
                    dependencyCollection.Add(dependencyAddress, dependencyImportOutput);
                    validDependencyIds.Add(dependencyAddress);
                } else {
                    BuildingOptions options = dependencyResourceLibrary.Registry[dependencyAddress.ResourceId].Option.Options;
                    
                    Importer importer = dependencyVertex.Importer;
                    if (Import(dependencyAddress, dependencyResourceLibrary, importer, options.Options, out var imported, out _)) {
                        dependencyVertex.SetObjectRepresentation(imported);
                        dependencyCollection.Add(dependencyAddress, imported);
                        validDependencyIds.Add(dependencyAddress);
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
        out FailureResult failureResult
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
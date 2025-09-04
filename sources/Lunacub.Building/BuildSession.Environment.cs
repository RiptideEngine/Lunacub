// ReSharper disable VariableHidesOuterVariable
namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources(bool rebuild) {
        Log.BeginBuildingEnvironmentResources(_environment.Logger);
        
        // Initialize some variable to use during graph cycle validation.
        HashSet<ResourceAddress> temporaryMarks = [], permanentMarks = [];
        Stack<ResourceAddress> cyclePath = [];
        
        // Populate the graph vertices.
        CreateGraphVertices(rebuild);

        if (Results.Count > 0) return;

        bool cycleDetected = false;
        CheckGraphCycle(_ => {
            Log.WarnGraphCycleDetectedAfterPopulateVerticesFromIncrementalInfos(_environment.Logger);
            cycleDetected = true;
        });

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
        
        // Alright everything has been populated, begin building resources.
        InnerBuildEnvironmentResources();
        
        Debug.Assert(
            _graph.Values.SelectMany(x => x.Vertices.Values).All(x => x.ObjectRepresentation == null), 
            "Resource leaked after building environment resources."
        );
        
        // We're done building environment resources, begin building procedural resources.
        BuildProceduralResources();
        
        // // All of the vertices has been populated.
        // // Better validate that we do not have a cycle in the dependency graph. I
        // // If we happen to have any, trigger rebuild.
        //
        // bool shouldRebuild = false;
        // CheckGraphCycle(_ => {
        //     _environment.Logger.LogWarning("Cycle detected after loading incremental informations. Rebuilding everything...");
        //     shouldRebuild = true;
        // });
        //
        // if (shouldRebuild) {
        //     throw new NotImplementedException();
        // } else {
        //     // We now proceed to extract the dependencies from the changed resources.
        //     // Recursively searching for the lowest changed resource in the hierarchy, and go up from there.
        //
        //     Parallel.ForEach(_graph, (graphKvp, _, _) => {
        //         (var libraryId, (var library, var vertexDictionary)) = graphKvp;
        //
        //         foreach ((ResourceID resourceId, ResourceRegistry.Element<BuildingResource> element) in library.Registry) {
        //             bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
        //             Debug.Assert(getSuccessfully && vertex != null);
        //
        //             if (vertex.IsSelfUnchanged) continue;
        //             if (vertex.Importer.Flags.HasFlag(ImporterFlags.NoDependency)) continue;
        //             
        //             try {
        //                 using SourceStreams streams = library.CreateSourceStreams(resourceId);
        //                 
        //                 try {
        //                     IReadOnlyCollection<ResourceAddress> extractedDependencies = vertex.Importer.ExtractDependencies(element.Option.Addresses, new());
        //
        //                     if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(address)) {
        //                         dependencyAddresses = dependencySet;
        //                     } else {
        //                         HashSet<ResourceAddress> createdSet = new(extractedDependencies);
        //                         createdSet.Remove(address);
        //                         dependencyAddresses = createdSet;
        //                     }
        //                 } catch (Exception e) {
        //                     vertex.MarkFaulty();
        //                     SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
        //                 }
        //             } catch (Exception e) {
        //                 vertex.MarkFaulty();
        //                 SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
        //             }
        //         }
        //     });
        // }
        //
        // CheckGraphCycle(path => {
        //     string message = string.Format(ExceptionMessages.CircularResourceDependency, string.Join(" -> ", path.Reverse()));
        //     throw new InvalidOperationException(message);
        // });
        //
        // // Parallel.ForEach(_graph, (graphKvp, _, _) => {
        // //     (LibraryID libraryId, LibraryGraphVertices vertices) = graphKvp;
        // //     BuildResourceLibrary library = graphKvp.Value.Library;
        // //
        // //     foreach ((ResourceID resourceId, ResourceRegistry.Element<BuildingResource> element) in library.Registry) {
        // //         ResourceAddress address = new(libraryId, resourceId);
        // //         
        // //         bool getSuccessfully = vertices.Vertices.TryGetValue(resourceId, out ResourceVertex? vertex);
        // //         Debug.Assert(getSuccessfully && vertex != null);
        // //
        // //         if (Interlocked.CompareExchange(ref vertex.DependenciesChangeStatus, DependenciesChangeStatus.Processing, DependenciesChangeStatus.Unknown) != DependenciesChangeStatus.Unknown) {
        // //             continue;
        // //         }
        // //
        // //         // if (vertex.IsSelfUnchanged) {
        // //         //     // If the resource itself is unchanged, no need to extract the dependencies from the sources.
        // //         //     continue;
        // //         // }
        // //         //
        // //         // // Early skip if the importer has NoDependency flag.
        // //         // if (vertex.Importer.Flags.HasFlag(ImporterFlags.NoDependency)) continue;
        // //         //
        // //         // using SourceStreams streams = library.CreateSourceStreams(resourceId);
        // //         //
        // //         // if (streams.PrimaryStream == null) {
        // //         //     SetResult(libraryId, resourceId, new(BuildStatus.NullPrimaryResourceStream));
        // //         //     continue;
        // //         // }
        // //         //
        // //         // foreach ((_, var secondaryStream) in streams.SecondaryStreams) {
        // //         //     if (secondaryStream == null) {
        // //         //         SetResult(libraryId, resourceId, new(BuildStatus.NullSecondaryResourceStream));
        // //         //         break;
        // //         //     }
        // //         // }
        // //         //
        // //         // if (TryGetResult(libraryId, resourceId, out _)) continue;
        // //         //
        // //         // IReadOnlySet<ResourceAddress> address;
        // //         //
        // //         // try {
        // //         //     IReadOnlyCollection<ResourceAddress> extractedDependencies = vertex.Importer.ExtractDependencies(element.Option.Addresses, new());
        // //         //
        // //         //     if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(address)) {
        // //         //         dependencyAddresses = dependencySet;
        // //         //     } else {
        // //         //         HashSet<ResourceAddress> createdSet = new(extractedDependencies);
        // //         //         createdSet.Remove(address);
        // //         //         dependencyAddresses = createdSet;
        // //         //     }
        // //         // } catch (Exception e) {
        // //         //     SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
        // //         //     continue;
        // //         // }
        // //     }
        // // });
        //
        // // foreach ((LibraryID libraryId, (BuildResourceLibrary library, Dictionary<ResourceID, ResourceVertex> vertices)) in _graph) {
        // //     foreach ((ResourceID resourceId, ResourceRegistry.Element<BuildingResource> element) in library.Registry) {
        // //         BuildingResource resource = element.Option;
        // //
        // //         // Check if we got the report from previous building session.
        // //         if (_environment.IncrementalInfos.TryGetValue(libraryId, out LibraryIncrementalInfos? libraryIncrementalInfos)) {
        // //             if (libraryIncrementalInfos.TryGetValue(resourceId, out IncrementalInfo previousIncrementalInfo)) {
        // //                 // So we do got the report from the last building session.
        // //
        // //                 // Get the source informations.
        // //                 SourcesInfo sourcesInformation;
        // //
        // //                 try {
        // //                     sourcesInformation = library.GetSourcesInformations(resourceId);
        // //                 } catch (Exception e) {
        // //                     SetResult(libraryId, resourceId, new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
        // //                     continue;
        // //                 }
        // //
        // //                 // Check the sources integrity:
        // //                 if (CheckSourcesIntegrity(previousIncrementalInfo.SourcesInfo, sourcesInformation)) {
        // //                     continue;
        // //                 }
        // //
        // //                 // Since the sources has no changes, the resources should still have the same dependencies.
        // //                 // Now check the integrity of the dependencies.
        // //
        // //
        // //                 // if (previousIncrementalInfo.SourcesLastWriteTime)
        // //
        // //                 // SourcesLastWriteTime lastWriteTimes;
        // //                 //
        // //                 // try {
        // //                 //     lastWriteTimes = library.GetSourceLastWriteTimes(resourceId);
        // //                 // } catch (Exception e) {
        // //                 //     SetResult(libraryId, resourceId, new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
        // //                 //     continue;
        // //                 // }
        // //
        // //
        // //             }
        // //         }
        // //
        // //         if (!_environment.Importers.TryGetValue(resource.Options.ImporterName, out var importer)) {
        // //             SetResult(libraryId, resourceId, new(BuildStatus.UnknownImporter));
        // //             continue;
        // //         }
        // //
        // //         try {
        // //             importer.ValidateResource(resource);
        // //         } catch (Exception e) {
        // //             SetResult(libraryId, resourceId, new(BuildStatus.InvalidBuildingResource, ExceptionDispatchInfo.Capture(e)));
        // //             continue;
        // //         }
        // //
        // //         IReadOnlySet<ResourceAddress> dependencyAddresses;
        // //
        // //         if (importer.Flags.HasFlag(ImporterFlags.NoDependency)) {
        // //             dependencyAddresses = FrozenSet<ResourceAddress>.Empty;
        // //         } else {
        // //             using SourceStreams streams = library.CreateSourceStreams(resourceId);
        // //
        // //             if (streams.PrimaryStream == null) {
        // //                 SetResult(libraryId, resourceId, new(BuildStatus.NullPrimaryResourceStream));
        // //                 continue;
        // //             }
        // //
        // //             foreach ((_, var secondaryStream) in streams.SecondaryStreams) {
        // //                 if (secondaryStream == null) {
        // //                     SetResult(libraryId, resourceId, new(BuildStatus.NullSecondaryResourceStream));
        // //                     break;
        // //                 }
        // //             }
        // //
        // //             if (TryGetResult(libraryId, resourceId, out _)) continue;
        // //
        // //             try {
        // //                 IReadOnlyCollection<ResourceAddress> extractedDependencies = importer.ExtractDependencies(streams);
        // //
        // //                 if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(new(libraryId, resourceId))) {
        // //                     dependencyAddresses = dependencySet;
        // //                 } else {
        // //                     HashSet<ResourceAddress> createdSet = new(extractedDependencies);
        // //                     createdSet.Remove(new(libraryId, resourceId));
        // //                     dependencyAddresses = createdSet;
        // //                 }
        // //             } catch (Exception e) {
        // //                 SetResult(libraryId, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
        // //                 continue;
        // //             }
        // //         }
        // //
        // //         vertices.Add(resourceId, new(importer, dependencyAddresses));
        // //     }
        // // }
        // //
        // // // Validate dependency graph.
        // // ValidateGraph();
        // //
        // // // Assigning reference count for dependencies.
        // // Parallel.ForEach(Partitioner.Create(_graph), (kvp, state, index) => {
        // //     var libraryVertices = kvp.Value;
        // //
        // //     foreach ((_, var vertex) in libraryVertices.Vertices) {
        // //         foreach (var dependencyAddress in vertex.DependencyResourceAddresses) {
        // //             if (!TryGetVertex(dependencyAddress, out var dependencyVertex)) continue;
        // //
        // //             dependencyVertex.IncrementReference();
        // //         }
        // //
        // //         vertex.IncrementReference();
        // //     }
        // // });
        // //
        // // // Handle the building procedure.
        // // foreach ((var libraryId, (var library, var libraryVertices)) in _graph) {
        // //     foreach ((var resourceId, var resourceVertex) in libraryVertices) {
        // //         BuildEnvironmentResource(new(libraryId, resourceId), library, resourceVertex, out _);
        // //     }
        // // }
        // //
        // // Debug.Assert(_graph.Values.SelectMany(x => x.Vertices.Values).All(x => x.ReferenceCount == 0));
        // return;
        //
        //
        // bool AreDependenciesChangedRecursively(IEnumerable<ResourceAddress> addresses) {
        //     foreach (ResourceAddress address in addresses) {
        //         if (_graph.TryGetValue(address.LibraryId, out var libraryGraphVertices)) {
        //             if (libraryGraphVertices.Vertices.TryGetValue(address.ResourceId, out var vertex)) {
        //                 if (vertex.IsSelfUnchanged) continue;
        //
        //                 return true;
        //             }
        //         }
        //     }
        //
        //     return false;
        // }
        //
        // bool AreDependenciesChanged(IEnumerable<ResourceAddress> addresses) {
        //     foreach (ResourceAddress address in addresses) {
        //         if (_graph.TryGetValue(address.LibraryId, out var libraryGraphVertices)) {
        //             if (libraryGraphVertices.Vertices.TryGetValue(address.ResourceId, out var vertex)) {
        //                 if (vertex.IsSelfUnchanged) continue;
        //
        //                 return true;
        //             }
        //         }
        //     }
        //
        //     return false;
        // }
        
        void CheckGraphCycle(Action<IEnumerable<ResourceAddress>> onCycleDetected) {
            if (_graph.Count == 0) return;
        
            try {
                foreach ((var libraryId, var libraryVertices) in _graph) {
                    foreach ((var resourceId, var resourceVertex) in libraryVertices.Vertices) {
                        if (Visit(new(libraryId, resourceId), resourceVertex)) {
                            return;
                        }
                    }
                }
            } finally {
                temporaryMarks.Clear();
                permanentMarks.Clear();
                cyclePath.Clear();
            }
        
            return;
        
            bool Visit(
                ResourceAddress resourceAddress,
                ResourceVertex resourceVertex
            ) {
                if (permanentMarks.Contains(resourceAddress)) return false;
        
                cyclePath.Push(resourceAddress);
        
                if (!temporaryMarks.Add(resourceAddress)) {
                    onCycleDetected(cyclePath.Reverse());
                    return true;
                }

                if (resourceVertex.DependencyResourceAddresses != null) {
                    foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses) {
                        if (!TryGetVertex(dependencyAddress, out var dependencyVertex)) continue;

                        if (Visit(dependencyAddress, dependencyVertex)) {
                            return true;
                        }
                    }
                }

                permanentMarks.Add(resourceAddress);
                cyclePath.Pop();
                return false;
            }
        }
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
                
                if (!rebuild && _environment.IncrementalInfos.TryGetValue(libraryId, out LibraryIncrementalInfos? libraryIncrementalInfos)) {
                    if (libraryIncrementalInfos.TryGetValue(resourceId, out IncrementalInfo previousIncrementalInfo)) {
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
            (var libraryId, (var library, var vertexDictionary)) = graphKvp;

            foreach ((ResourceID resourceId, _) in library.Registry) {
                ResourceAddress address = new(libraryId, resourceId);
                
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex != null);
                Debug.Assert(vertex.DependencyResourceAddresses != null);

                foreach (var dependencyAddress in vertex.DependencyResourceAddresses) {
                    if (!TryGetVertex(dependencyAddress, out _, out _)) continue;
                    
                    vertex.IncrementReference();
                }
                
                // Self-reference.
                vertex.IncrementReference();
            }
        });
    }

    private void InnerBuildEnvironmentResources() {
        foreach ((var libraryId, (BuildResourceLibrary library, var vertexDictionary)) in _graph) {
            foreach ((var resourceId, var element) in library.Registry) {
                bool getSuccessfully = vertexDictionary.TryGetValue(resourceId, out ResourceVertex? vertex);
                Debug.Assert(getSuccessfully && vertex is not null);
                
                BuildSingleResource(new(libraryId, resourceId), library, vertex);
            }
        }
    }

    private void BuildSingleResource(
        ResourceAddress resourceAddress,
        BuildResourceLibrary resourceLibrary,
        ResourceVertex resourceVertex
    ) {
        FailureResult failureResult;
        
        if (resourceVertex.IsBuilt) return;

        Debug.Assert(resourceAddress.LibraryId == resourceLibrary.Id);
        
        bool dependencyRebuilt = false;
        foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses) {
            if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;
            
            BuildSingleResource(dependencyAddress, dependencyResourceLibrary, dependencyVertex);
        
            if (!dependencyVertex.IsSelfUnchanged) {
                dependencyRebuilt = true;
            }
        }

        try {
            ResourceRegistry.Element<BuildingResource> registryElement = resourceLibrary.Registry[resourceAddress.ResourceId];
            BuildingOptions options = registryElement.Option.Options;

            var importer = resourceVertex.Importer;

            if (!dependencyRebuilt && resourceVertex.IsSelfUnchanged) return;
            
            // Import if haven't.
            if (resourceVertex.ObjectRepresentation == null) {
                if (!Import(resourceAddress, resourceLibrary, importer, options.Options, out var importOutput, out failureResult)) {
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
                    resourceVertex.DependencyResourceAddresses,
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
            
            _environment.IncrementalInfos.SetIncrementalInfo(resourceAddress, new(resourceVertex.SourcesInformation, options, dependencyIds, new(importer.Version, resourceVertex.Processor?.Version)));
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
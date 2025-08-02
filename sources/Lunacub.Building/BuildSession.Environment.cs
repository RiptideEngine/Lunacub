using Caxivitual.Lunacub.Building.Exceptions;
using Caxivitual.Lunacub.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
// ReSharper disable VariableHidesOuterVariable

namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources() {
        foreach (var library in _environment.Libraries) {
            foreach ((var resourceId, var element) in library.Registry) {
                BuildingResource resource = element.Option;
                
                if (!_environment.Importers.TryGetValue(resource.Options.ImporterName, out var importer)) {
                    SetResult(library.Id, resourceId, new(BuildStatus.UnknownImporter));
                    continue;
                }

                try {
                    importer.ValidateResource(resource);
                } catch (Exception e) {
                    SetResult(library.Id, resourceId, new(BuildStatus.InvalidBuildingResource, ExceptionDispatchInfo.Capture(e)));
                    continue;
                }

                IReadOnlySet<ResourceAddress> dependencyAddresses;
                
                if (importer.Flags.HasFlag(ImporterFlags.NoDependency)) {
                    dependencyAddresses = FrozenSet<ResourceAddress>.Empty;
                } else {
                    using SourceStreams streams = library.CreateSourceStreams(resourceId);
                    
                    if (streams.PrimaryStream == null) {
                        SetResult(library.Id, resourceId, new(BuildStatus.NullPrimaryResourceStream));
                        continue;
                    }

                    foreach ((_, var secondaryStream) in streams.SecondaryStreams) {
                        if (secondaryStream == null) {
                            SetResult(library.Id, resourceId, new(BuildStatus.NullSecondaryResourceStream));
                            break;
                        }
                    }

                    if (TryGetResult(library.Id, resourceId, out _)) continue;
                    
                    try {
                        IReadOnlyCollection<ResourceAddress> extractedDependencies = importer.ExtractDependencies(streams);

                        if (extractedDependencies is IReadOnlySet<ResourceAddress> dependencySet && !dependencySet.Contains(new(library.Id, resourceId))) {
                            dependencyAddresses = dependencySet;
                        } else {
                            HashSet<ResourceAddress> createdSet = new(extractedDependencies);
                            createdSet.Remove(new(library.Id, resourceId));
                            dependencyAddresses = createdSet;
                        }
                    } catch (Exception e) {
                        SetResult(library.Id, resourceId, new(BuildStatus.ExtractDependenciesFailed, ExceptionDispatchInfo.Capture(e)));
                        continue;
                    }
                }

                _graph.Add(new(library.Id, resourceId), new(library, dependencyAddresses, element));
            }
        }

        // Validate dependency graph.
        ValidateGraph();
        
        // Assigning reference count for dependencies.
        foreach ((_, var vertex) in _graph) {
            foreach (var dependencyAddress in vertex.DependencyResourceAddresses) {
                if (!_graph.TryGetValue(dependencyAddress, out EnvironmentResourceVertex? dependencyVertex)) continue;
                
                dependencyVertex.ReferenceCount++;
            }

            vertex.ReferenceCount++;    // Environment resources need the import output to process and serialize.
        }

        // Handle the building procedure.
        foreach ((var resourceAddress, var vertex) in _graph) {
            BuildEnvironmentResource(resourceAddress, vertex, out _);
        }
        
        Debug.Assert(_graph.Values.All(x => x.ReferenceCount == 0));
        
        void ValidateGraph() {
            if (_graph.Count == 0) return;
        
            HashSet<ResourceAddress> temporaryMarks = [], permanentMarks = [];
            Stack<ResourceAddress> path = [];
            
            foreach ((var resourceAddress, _) in _graph) {
                Visit(resourceAddress, temporaryMarks, permanentMarks, path);
            }

            void Visit(
                ResourceAddress resourceAddress,
                HashSet<ResourceAddress> temporaryMarks,
                HashSet<ResourceAddress> permanentMarks,
                Stack<ResourceAddress> path
            ) {
                if (!_graph.ContainsKey(resourceAddress)) return;
                if (permanentMarks.Contains(resourceAddress)) return;
                
                path.Push(resourceAddress);
                
                if (!temporaryMarks.Add(resourceAddress)) {
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }
                
                foreach (var dependencyID in _graph[resourceAddress].DependencyResourceAddresses) {
                    Visit(dependencyID, temporaryMarks, permanentMarks, path);
                }
                
                permanentMarks.Add(resourceAddress);
                path.Pop();
            }
        }
    }
    
    private void BuildEnvironmentResource(
        ResourceAddress address,
        EnvironmentResourceVertex resourceVertex,
        out ResourceBuildingResult outputResult
    ) {
        if (TryGetResult(address, out outputResult)) return;
        
        Debug.Assert(resourceVertex.Library.Id == address.LibraryId);
        
        bool dependencyRebuilt = false;
        foreach (var dependencyId in resourceVertex.DependencyResourceAddresses) {
            if (!_graph.TryGetValue(dependencyId, out var dependencyVertexInfo)) continue;
            
            BuildEnvironmentResource(dependencyId, dependencyVertexInfo, out ResourceBuildingResult dependencyResult);

            if (dependencyResult.Status == BuildStatus.Success) {
                dependencyRebuilt = true;
            }
        }

        try {
            ResourceRegistry.Element<BuildingResource> registryElement = resourceVertex.RegistryElement;
            BuildingOptions options = registryElement.Option.Options;

            Importer importer = _environment.Importers[options.ImporterName];

            string? processorName = options.ProcessorName;
            Processor? processor = null;

            if (!string.IsNullOrEmpty(processorName) && !_environment.Processors.TryGetValue(processorName, out processor)) {
                SetResult(address, outputResult = new(BuildStatus.UnknownProcessor));
                return;
            }

            SourceLastWriteTimes lastWriteTimes;

            try {
                lastWriteTimes = resourceVertex.Library.GetSourceLastWriteTimes(address.ResourceId);
            } catch (Exception e) {
                SetResult(address, outputResult = new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }
            
            if (!dependencyRebuilt) {
                if (IsResourceCacheable(address, lastWriteTimes, options, resourceVertex.DependencyResourceAddresses, out var previousIncrementalInfo)) {
                    if (new ComponentVersions(importer.Version, processor?.Version) == previousIncrementalInfo.ComponentVersions) {
                        SetResult(address, outputResult = new(BuildStatus.Cached));
                        AddOutputResourceRegistry(address, new(registryElement.Name, registryElement.Tags));
                        return;
                    }
                }
            }
            
            // Import if haven't.
            if (resourceVertex.ImportOutput == null) {
                if (!Import(address, resourceVertex.Library, importer, options.Options, out var importOutput, out outputResult)) {
                    return;
                }
                
                resourceVertex.SetImportResult(importOutput);
            }

            IReadOnlySet<ResourceAddress> dependencyIds;
            
            if (processor == null) {
                try {
                    SerializeProcessedObject(resourceVertex.ImportOutput, address, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(address, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }
                
                dependencyIds = FrozenSet<ResourceAddress>.Empty;
            } else {
                if (!processor.CanProcess(resourceVertex.ImportOutput)) {
                    SetResult(address, outputResult = new(BuildStatus.Unprocessable));
                    return;
                }

                CollectDependencies(
                    resourceVertex.DependencyResourceAddresses,
                    out IReadOnlyDictionary<ResourceAddress, ContentRepresentation> dependencies,
                    out IReadOnlySet<ResourceAddress> validDependencyIds
                );
                resourceVertex.DependencyResourceAddresses = validDependencyIds;

                ContentRepresentation processed;
                ProcessingContext processingContext;
                
                try {
                    processingContext = new(_environment, address, options.Options, dependencies, _environment.Logger);
                    processed = processor.Process(resourceVertex.ImportOutput, processingContext);
                } catch (Exception e) {
                    SetResult(address, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                }

                try {
                    SerializeProcessedObject(processed, address, options.Options, registryElement.Tags);
                } catch (Exception e) {
                    SetResult(address, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
                    return;
                } finally {
                    if (!ReferenceEquals(resourceVertex.ImportOutput, processed)) {
                        processed.Dispose();
                    }
                }
                
                AppendProceduralResources(address.LibraryId, processingContext.ProceduralResources, _proceduralResources);
                dependencyIds = validDependencyIds;
            }

            SetResult(address, outputResult = new(BuildStatus.Success));
            _environment.IncrementalInfos.SetIncrementalInfo(address, new(lastWriteTimes, options, dependencyIds, new(importer.Version, processor?.Version)));
            AddOutputResourceRegistry(address, new(registryElement.Name, registryElement.Tags));
        } finally {
            ReleaseDependencies(resourceVertex.DependencyResourceAddresses);
            resourceVertex.Release();
        }

        return;

        void CollectDependencies(
            IReadOnlyCollection<ResourceAddress> dependencyAddresses,
            out IReadOnlyDictionary<ResourceAddress, ContentRepresentation> collectedDependencies,
            out IReadOnlySet<ResourceAddress> validDependencies
        ) {
            if (dependencyAddresses.Count == 0) {
                collectedDependencies = FrozenDictionary<ResourceAddress, ContentRepresentation>.Empty;
                validDependencies = FrozenSet<ResourceAddress>.Empty;
                return;
            }
        
            Dictionary<ResourceAddress, ContentRepresentation> dependencyCollection = [];
            HashSet<ResourceAddress> validDependencyIds = [];

            foreach (var dependencyAddress in dependencyAddresses) {
                if (!_graph.TryGetValue(dependencyAddress, out EnvironmentResourceVertex? dependencyVertex)) continue;

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
                        
                        BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;

                        Importer importer = _environment.Importers[options.ImporterName];
                        if (Import(dependencyAddress, dependencyVertex.Library, importer, options.Options, out var imported, out _)) {
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
        [NotNullWhen(true)] out ContentRepresentation? imported,
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
// ReSharper disable VariableHidesOuterVariable
namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void BuildEnvironmentResources() {
        
    }

    private void BuildEnvironmentResource(
        ResourceAddress resourceAddress,
        BuildResourceLibrary resourceLibrary,
        ResourceVertex resourceVertex,
        out ResourceBuildingResult outputResult
    ) {
        throw new NotImplementedException();
        // if (TryGetResult(resourceAddress, out outputResult)) return;
        //
        // Debug.Assert(resourceAddress.LibraryId == resourceLibrary.Id);
        //
        // bool dependencyRebuilt = false;
        // foreach (var dependencyAddress in resourceVertex.DependencyResourceAddresses) {
        //     if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;
        //     
        //     BuildEnvironmentResource(dependencyAddress, dependencyResourceLibrary, dependencyVertex, out ResourceBuildingResult dependencyResult);
        //
        //     if (dependencyResult.Status == BuildStatus.Success) {
        //         dependencyRebuilt = true;
        //     }
        // }
        //
        // try {
        //     // ResourceRegistry.Element<BuildingResource> registryElement = resourceVertex.RegistryElement;
        //     ResourceRegistry.Element<BuildingResource> registryElement = resourceLibrary.Registry[resourceAddress.ResourceId];
        //     BuildingOptions options = registryElement.Option.Options;
        //
        //     string? processorName = options.ProcessorName;
        //     Processor? processor = null;
        //
        //     if (!string.IsNullOrEmpty(processorName) && !_environment.Processors.TryGetValue(processorName, out processor)) {
        //         SetResult(resourceAddress, outputResult = new(BuildStatus.UnknownProcessor));
        //         return;
        //     }
        //
        //     SourcesLastWriteTime lastWriteTimes;
        //
        //     try {
        //         lastWriteTimes = resourceLibrary.GetSourceLastWriteTimes(resourceAddress.ResourceId);
        //     } catch (Exception e) {
        //         SetResult(resourceAddress, outputResult = new(BuildStatus.GetSourceLastWriteTimesFailed, ExceptionDispatchInfo.Capture(e)));
        //         return;
        //     }
        //
        //     var importer = resourceVertex.Importer;
        //     
        //     if (!dependencyRebuilt) {
        //         if (IsResourceCacheable(resourceAddress, lastWriteTimes, options, resourceVertex.DependencyResourceAddresses, out var previousIncrementalInfo)) {
        //             if (new ComponentVersions(importer.Version, processor?.Version) == previousIncrementalInfo.ComponentVersions) {
        //                 SetResult(resourceAddress, outputResult = new(BuildStatus.Cached));
        //                 AddOutputResourceRegistry(resourceAddress, new(registryElement.Name, registryElement.Tags));
        //                 return;
        //             }
        //         }
        //     }
        //     
        //     // Import if haven't.
        //     if (resourceVertex.ImportedData == null) {
        //         if (!Import(resourceAddress, resourceLibrary, importer, options.Options, out var importOutput, out outputResult)) {
        //             return;
        //         }
        //         
        //         resourceVertex.SetImportResult(importOutput);
        //     }
        //
        //     IReadOnlySet<ResourceAddress> dependencyIds;
        //     
        //     if (processor == null) {
        //         try {
        //             SerializeProcessedObject(resourceVertex.ImportedData, resourceAddress, options.Options, registryElement.Tags);
        //         } catch (Exception e) {
        //             SetResult(resourceAddress, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
        //             return;
        //         }
        //         
        //         dependencyIds = FrozenSet<ResourceAddress>.Empty;
        //     } else {
        //         if (!processor.CanProcess(resourceVertex.ImportedData)) {
        //             SetResult(resourceAddress, outputResult = new(BuildStatus.Unprocessable));
        //             return;
        //         }
        //
        //         CollectDependencies(
        //             resourceVertex.DependencyResourceAddresses,
        //             out IReadOnlyDictionary<ResourceAddress, object> dependencies,
        //             out IReadOnlySet<ResourceAddress> validDependencyIds
        //         );
        //         resourceVertex.DependencyResourceAddresses = validDependencyIds;
        //
        //         object processed;
        //         ProcessingContext processingContext;
        //         
        //         try {
        //             processingContext = new(_environment, resourceAddress, options.Options, dependencies, _environment.Logger);
        //             processed = processor.Process(resourceVertex.ImportedData, processingContext);
        //         } catch (Exception e) {
        //             SetResult(resourceAddress, outputResult = new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
        //             return;
        //         }
        //
        //         try {
        //             SerializeProcessedObject(processed, resourceAddress, options.Options, registryElement.Tags);
        //         } catch (Exception e) {
        //             SetResult(resourceAddress, outputResult = new(BuildStatus.SerializationFailed, ExceptionDispatchInfo.Capture(e)));
        //             return;
        //         } finally {
        //             if (!ReferenceEquals(resourceVertex.ImportedData, processed)) {
        //                 processor.Dispose(processed, new(_environment.Logger));
        //             }
        //         }
        //         
        //         AppendProceduralResources(resourceAddress, processingContext.ProceduralResources, _proceduralResources);
        //         dependencyIds = validDependencyIds;
        //     }
        //
        //     SetResult(resourceAddress, outputResult = new(BuildStatus.Success));
        //     _environment.IncrementalInfos.SetIncrementalInfo(resourceAddress, new(lastWriteTimes, options, dependencyIds, new(importer.Version, processor?.Version)));
        //     AddOutputResourceRegistry(resourceAddress, new(registryElement.Name, registryElement.Tags));
        // } finally {
        //     ReleaseDependencies(resourceVertex.DependencyResourceAddresses);
        //     ReleaseVertexOutput(resourceVertex);
        // }
        //
        // return;
        //
        // void CollectDependencies(
        //     IReadOnlyCollection<ResourceAddress> dependencyAddresses,
        //     out IReadOnlyDictionary<ResourceAddress, object> collectedDependencies,
        //     out IReadOnlySet<ResourceAddress> validDependencies
        // ) {
        //     if (dependencyAddresses.Count == 0) {
        //         collectedDependencies = FrozenDictionary<ResourceAddress, object>.Empty;
        //         validDependencies = FrozenSet<ResourceAddress>.Empty;
        //         return;
        //     }
        //
        //     Dictionary<ResourceAddress, object> dependencyCollection = [];
        //     HashSet<ResourceAddress> validDependencyIds = [];
        //
        //     foreach (var dependencyAddress in dependencyAddresses) {
        //         if (!TryGetVertex(dependencyAddress, out var dependencyResourceLibrary, out var dependencyVertex)) continue;
        //
        //         if (dependencyVertex.ImportedData is { } dependencyImportOutput) {
        //             dependencyCollection.Add(dependencyAddress, dependencyImportOutput);
        //             validDependencyIds.Add(dependencyAddress);
        //         } else {
        //             if (TryGetResult(dependencyAddress, out var result)) {
        //                 // Traversed through this resource vertex before.
        //                 switch (result.Status) {
        //                     case BuildStatus.Success:
        //                         Debug.Assert(dependencyVertex.ImportedData != null);
        //                     
        //                         dependencyCollection[dependencyAddress] = dependencyVertex.ImportedData;
        //                         validDependencyIds.Add(dependencyAddress);
        //                         continue;
        //                 
        //                     case BuildStatus.Cached:
        //                         // Resource is cached, but still import it to handle dependencies.
        //                         // Fallthrough.
        //                         break;
        //                 
        //                     default: continue;
        //                 }
        //                 
        //                 // BuildingOptions options = dependencyVertex.RegistryElement.Option.Options;
        //                 BuildingOptions options = dependencyResourceLibrary.Registry[dependencyAddress.ResourceId].Option.Options;
        //
        //                 Importer importer = dependencyVertex.Importer;
        //                 if (Import(dependencyAddress, dependencyResourceLibrary, importer, options.Options, out var imported, out _)) {
        //                     dependencyVertex.SetImportResult(imported);
        //                     dependencyCollection.Add(dependencyAddress, imported);
        //                     validDependencyIds.Add(dependencyAddress);
        //                 }
        //             }
        //         }
        //     }
        //
        //     collectedDependencies = dependencyCollection;
        //     validDependencies = validDependencyIds;
        // }
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
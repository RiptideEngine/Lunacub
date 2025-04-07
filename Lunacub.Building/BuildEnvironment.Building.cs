using System.Buffers.Binary;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        DateTime start = DateTime.Now;

        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        foreach ((var rid, var buildingResource) in Resources) {
            BuildResource(rid, in buildingResource, results);
        }

        return new(start, DateTime.Now, results);
    }

    public BuildingResult BuildResource(ResourceID rid) {
        DateTime start = DateTime.Now;
        
        if (!Resources.TryGet(rid, out ResourceRegistry.BuildingResource buildingResource)) {
            return new(start, start, new Dictionary<ResourceID, ResourceBuildingResult> {
                [rid] = new(BuildStatus.ResourceNotFound),
            });
        }

        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        BuildResource(rid, in buildingResource, results);
        
        return new(start, DateTime.Now, results);
    }

    private void BuildResource(ResourceID rid, in ResourceRegistry.BuildingResource buildingResource, Dictionary<ResourceID, ResourceBuildingResult> results) {
        if (results.ContainsKey(rid)) return;

        (string resourcePath, BuildingOptions options) = buildingResource;
        
        DateTime resourceLastWriteTime = File.GetLastWriteTime(resourcePath);

        // If resource has been built before, and have old report, we can begin checking for caching.
        if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && IncrementalInfos.TryGet(rid, out var previousReport)) {
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousReport.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
                // If any importing option is different.
                if (options.Equals(previousReport.Options)) {
                    ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(results, rid, out bool exists);

                    if (!exists) {
                        reference = new(BuildStatus.Cached);
                    }
                    
                    return;
                }
            }
        }

        if (!Importers.TryGetValue(options.ImporterName, out Importer? importer)) {
            results.Add(rid, new(BuildStatus.UnknownImporter));
            return;
        }

        ImportingContext importingContext;

        try {
            using FileStream stream = File.OpenRead(resourcePath);

            importingContext = new(options.Options);
            using ContentRepresentation imported = importer.ImportObject(stream, importingContext);
            
            string? processorName = options.ProcessorName;

            Processor? processor;

            if (string.IsNullOrWhiteSpace(processorName)) {
                processor = Processor.Passthrough;
            } else if (!Processors.TryGetValue(processorName, out processor)) {
                results.Add(rid, new(BuildStatus.UnknownProcessor));
                return;
            }

            if (!processor.CanProcess(imported)) {
                results.Add(rid, new(BuildStatus.CannotProcess));
                return;
            }

            IncrementalInfo incrementalInfo = new(resourceLastWriteTime, options);

            ContentRepresentation processed;

            try {
                processed = processor.Process(imported, new(options.Options));
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }

            try {
                if (SerializerFactories.GetSerializableFactory(processed.GetType()) is not { } factory) {
                    throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializer, processed.GetType()));
                }
        
                using MemoryStream ms = new(4096);
                CompileHelpers.Compile(factory.InternalCreateSerializer(processed, new(options.Options)), ms);
                ms.Position = 0;
                Output.CopyCompiledResourceOutput(ms, rid);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.CompilationFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            } finally {
                processed.Dispose();
            }

            results.Add(rid, new(BuildStatus.Success));
            IncrementalInfos.Add(rid, incrementalInfo);
        } catch (Exception e) {
            results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
            return;
        }
        
        foreach (var reference in importingContext.References) {
            if (!Resources.TryGet(reference, out ResourceRegistry.BuildingResource buildingReference)) continue;

            BuildResource(reference, buildingReference, results);
        }
    }
}
using Caxivitual.Lunacub.Building.Collections;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        DateTime begin = DateTime.Now;
        Dictionary<ResourceID, ResourceBuildingResult> results = [];
    
        // Build dependency graph and build all the resources that play part in it.
        Dictionary<ResourceID, VertexInfo> graph = BuildDependencyGraph(results);
        try {
            foreach ((_, var info) in graph) {
                info.SetReferenceCount(info.DependentIds.Count);
            }
    
            // Enumerate through root nodes
            foreach ((var rid, var info) in graph) {
                if (info.DependentIds.Count != 0) continue;
    
                BuildResourceInDependecyGraph(rid, info, graph, results);
            }
        } finally {
            Debug.Assert(graph.Values.All(x => x.RefCount == 0));
        }
        
        // TODO: Build the remain resources.
        foreach ((var rid, var resource) in Resources) {
            BuildSingularResource(rid, resource, results, graph);
        }
        
        return new(begin, DateTime.Now, results);
    }
    
    private Dictionary<ResourceID, VertexInfo> BuildDependencyGraph(Dictionary<ResourceID, ResourceBuildingResult> results) {
        Dictionary<ResourceID, VertexInfo> outputVertices = [];
        
        // Collecting edges.
        foreach ((var rid, var resource) in Resources) {
            CollectRecursively(outputVertices, rid, resource, results);
        }
    
        ValidateGraph(outputVertices);
        
        return outputVertices;
    
        void CollectRecursively(Dictionary<ResourceID, VertexInfo> receiver, ResourceID rid, BuildingResource resource, Dictionary<ResourceID, ResourceBuildingResult> results) {
            if (receiver.ContainsKey(rid)) return;
    
            if (!Importers.TryGetValue(resource.Options.ImporterName, out Importer? importer)) {
                results.Add(rid, new(BuildStatus.UnknownImporter));
                return;
            }
            
            using var stream = resource.Provider.GetStream();
            var dependencies = importer.GetDependencies(stream).Where(FilterContainsResource).ToHashSet();
            dependencies.Remove(rid);   // Fix self-dependent.
            
            if (dependencies.Count == 0) return;
            
            receiver.Add(rid, new(dependencies, []));
    
            foreach (var dependency in dependencies) {
                CollectRecursivelyWithDependent(receiver, dependency, rid, results);
            }
        }
        
        void CollectRecursivelyWithDependent(Dictionary<ResourceID, VertexInfo> receiver, ResourceID rid, ResourceID dependent, Dictionary<ResourceID, ResourceBuildingResult> results) {
            if (receiver.TryGetValue(rid, out var edges)) {
                edges.DependentIds.Add(dependent);
            } else {
                if (!Resources.TryGetValue(rid, out var resource)) return;
                
                if (!Importers.TryGetValue(resource.Options.ImporterName, out Importer? importer)) {
                    results.Add(rid, new(BuildStatus.UnknownImporter));
                    return;
                }
    
                using var stream = resource.Provider.GetStream();
                var dependencies = importer.GetDependencies(stream).Where(FilterContainsResource).ToHashSet();
                dependencies.Remove(rid);   // Fix self-dependent.
                
                receiver.Add(rid, new(dependencies, [dependent]));
                
                foreach (var dependency in dependencies) {
                    CollectRecursivelyWithDependent(receiver, dependency, rid, results);
                }
            }
        }
        
        bool FilterContainsResource(ResourceID rid) => Resources.ContainsKey(rid);
        
        static void ValidateGraph(Dictionary<ResourceID, VertexInfo> vertices) {
            if (vertices.Count == 0) return;
            
            HashSet<ResourceID> temporaryMark = [], permanentMark = [];
            Stack<ResourceID> path = [];
            
            foreach ((var rid, _) in vertices) {
                Visit(rid, vertices, temporaryMark, permanentMark, path);
            }
    
            static void Visit(ResourceID rid, Dictionary<ResourceID, VertexInfo> vertices, HashSet<ResourceID> temporaryMark, HashSet<ResourceID> permanentMark, Stack<ResourceID> path) {
                if (permanentMark.Contains(rid)) return;
                
                path.Push(rid);
                
                if (!temporaryMark.Add(rid)) {
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }
                
                foreach (var dependencyID in vertices[rid].DependencyIds) {
                    Visit(dependencyID, vertices, temporaryMark, permanentMark, path);
                }
                
                permanentMark.Add(rid);
                path.Pop();
            }
        }
    }

    private void BuildResourceInDependecyGraph(ResourceID rid, VertexInfo vertexInfo, Dictionary<ResourceID, VertexInfo> graph, Dictionary<ResourceID, ResourceBuildingResult> results) {
        Debug.Assert(graph.ContainsKey(rid));
        bool tryget = Resources.TryGetValue(rid, out var buildingResource);
        Debug.Assert(tryget);

        if (vertexInfo.Visited) return;
        
        vertexInfo.Visited = true;
        
        bool rebuild = false;
        foreach (var dependencyId in vertexInfo.DependencyIds) {
            tryget = graph.TryGetValue(dependencyId, out var dependencyVertexInfo);
            Debug.Assert(tryget);
            
            BuildResourceInDependecyGraph(dependencyId, dependencyVertexInfo!, graph, results);

            if (!dependencyVertexInfo!.Cached) {
                rebuild = true;
            }
        }

        (ResourceProvider provider, BuildingOptions options) = buildingResource;
        DateTime resourceLastWriteTime = provider.LastWriteTime;
        
        if (!GetBuildingComponents(rid, options.ImporterName, out Importer? importer, options.ProcessorName, out Processor? processor, results)) {
            return;
        }

        if (!rebuild && IsResourceCacheable(rid, resourceLastWriteTime, options, vertexInfo.DependencyIds, out IncrementalInfo previousIncrementalInfo)) {
            if (new ComponentVersions(importer.Version, processor.Version) == previousIncrementalInfo.ComponentVersions) {
                // Cache the resource.
                
                vertexInfo.Cached = true;
                results.Add(rid, new(BuildStatus.Cached));
                
                ReleaseDependencies(vertexInfo.DependencyIds, graph);
                return;
            }
        }
        
        vertexInfo.Cached = false;  // Probably not needed but just in case lmao
        
        // Import if haven't.
        if (vertexInfo.ImportOutput == null) {
            if (!Import(rid, provider, options, importer, results, out ContentRepresentation? importOutput, out ImportingContext? context)) {
                vertexInfo.FaultyImport = true;
                return;
            }
            
            vertexInfo.SetImported(importOutput, context);
        }
        
        // Collect the dependencies.
        Dictionary<ResourceID, ContentRepresentation> dependencies = CollectDependencies(vertexInfo.DependencyIds, graph, results);
        
        try {
            if (!Process(rid, vertexInfo.ImportOutput, options, processor, dependencies, results, out ContentRepresentation? processed)) {
                return;
            }
            
            try {
                SerializeProcessedObject(processed, rid, options);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.CompilationFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            } finally {
                processed.Dispose();
            }
            
            results.Add(rid, new(BuildStatus.Success));
            IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, vertexInfo.DependencyIds, new(importer.Version, processor.Version)));
        } finally {
            ReleaseDependencies(vertexInfo.DependencyIds, graph);
        }

        static void ReleaseDependencies(IReadOnlyCollection<ResourceID> dependencyIds, Dictionary<ResourceID, VertexInfo> graph) {
            foreach (var dependencyId in dependencyIds) {
                bool tryget = graph.TryGetValue(dependencyId, out var dependencyVertexInfo);
                Debug.Assert(tryget);
                
                dependencyVertexInfo?.Release();
            }
        }
    }
    
    Dictionary<ResourceID, ContentRepresentation> CollectDependencies(IReadOnlySet<ResourceID> dependencyIds, Dictionary<ResourceID, VertexInfo> graph, Dictionary<ResourceID, ResourceBuildingResult> results) {
        Dictionary<ResourceID, ContentRepresentation> dependencies = new(dependencyIds.Count);
            
        foreach (var dependencyId in dependencyIds) {
            bool tryget = graph.TryGetValue(dependencyId, out var dependencyVertexInfo);
            Debug.Assert(tryget);
            Debug.Assert(dependencyVertexInfo!.Visited);

            if (dependencyVertexInfo.FaultyImport) {
                continue;
            }
                
            if (dependencyVertexInfo.Cached) {
                Debug.Assert(dependencyVertexInfo.ImportOutput == null);
                    
                tryget = Resources.TryGetValue(dependencyId, out var dependencyResource);
                Debug.Assert(tryget);
                
                (ResourceProvider provider, BuildingOptions options) = dependencyResource;
                
                if (!Importers.TryGetValue(options.ImporterName, out var importer) || !Import(dependencyId, provider, options, importer, results, out ContentRepresentation? importOutput, out ImportingContext? context)) {
                    dependencyVertexInfo.FaultyImport = true;
                    continue;
                }
                
                dependencyVertexInfo.SetImported(importOutput, context);
                dependencies.Add(dependencyId, importOutput);
                continue;
            }
                
            Debug.Assert(dependencyVertexInfo.ImportOutput != null);
            dependencies.Add(dependencyId, dependencyVertexInfo.ImportOutput);
        }
            
        return dependencies;
    }
    
    private void BuildSingularResource(ResourceID rid, BuildingResource resource, Dictionary<ResourceID, ResourceBuildingResult> results, Dictionary<ResourceID, VertexInfo> graph) {
        if (graph.ContainsKey(rid)) return;
        
        if (results.ContainsKey(rid)) return;
        
        (ResourceProvider provider, BuildingOptions options) = resource;
        DateTime resourceLastWriteTime = provider.LastWriteTime;

        if (!GetBuildingComponents(rid, options.ImporterName, out Importer? importer, options.ProcessorName, out Processor? processor, results)) {
            return;
        }
        
        if (IsResourceCacheable(rid, resourceLastWriteTime, options, FrozenSet<ResourceID>.Empty, out var previousIncrementalInfo)) {
            if (new ComponentVersions(importer.Version, processor.Version) == previousIncrementalInfo.ComponentVersions) {
                results.Add(rid, new(BuildStatus.Cached));
                return;
            }
        }

        if (!Import(rid, provider, options, importer, results, out ContentRepresentation? imported, out ImportingContext? context)) {
            return;
        }

        if (!Process(rid, imported, options, processor, FrozenDictionary<ResourceID, ContentRepresentation>.Empty, results, out var processed)) {
            return;
        }
        
        try {
            SerializeProcessedObject(processed, rid, options);
        } catch (Exception e) {
            results.Add(rid, new(BuildStatus.CompilationFailed, ExceptionDispatchInfo.Capture(e)));
            return;
        } finally {
            processed.Dispose();
        }

        results.Add(rid, new(BuildStatus.Success));
        IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, FrozenSet<ResourceID>.Empty, new(importer.Version, processor.Version)));

        foreach (var reference in context.References) {
            if (!Resources.TryGetValue(reference, out var referenceResource)) continue;
            
            BuildSingularResource(reference, referenceResource, results, graph);
        }
    }
    
    /// <summary>
    /// Determines whether a resource should be rebuilt based on timeline, configurations, dependencies from previous build informations.
    /// </summary>
    /// <param name="rid">Resource to determines whether rebuilding needed.</param>
    /// <param name="resourceLastWriteTime">The last write time of resource.</param>
    /// <param name="currentOptions">Building options of the resource.</param>
    /// <param name="currentDependencies">Dependencies of the resource.</param>
    /// <param name="previousIncrementalInfo">
    ///     When this method returns, contains the <see cref="IncrementalInfo"/> of the previous building session of the resource. If the
    ///     resource hasn't been build before, <see langword="default"/> is returned.
    /// </param>
    /// <returns><see langword="true"/> if the resource should be rebuilt; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The function does not account for the version of building components.</remarks>
    private bool IsResourceCacheable(ResourceID rid, DateTime resourceLastWriteTime, BuildingOptions currentOptions, IReadOnlySet<ResourceID> currentDependencies, out IncrementalInfo previousIncrementalInfo) {
        // If resource has been built before, and have old report, we can begin checking for caching.
        if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && IncrementalInfos.TryGet(rid, out previousIncrementalInfo)) {
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousIncrementalInfo.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
                // If the options are equal, no need to rebuild.
                if (currentOptions.Equals(previousIncrementalInfo.Options)) {
                    if (previousIncrementalInfo.Dependencies.SequenceEqual(currentDependencies)) {
                        return true;
                    }
                }
            }
    
            return false;
        }
    
        previousIncrementalInfo = default;
        return false;
    }

    private bool Import(ResourceID rid, ResourceProvider provider, BuildingOptions options, Importer importer, Dictionary<ResourceID, ResourceBuildingResult> results, [NotNullWhen(true)] out ContentRepresentation? imported, [NotNullWhen(true)] out ImportingContext? context) {
        Debug.Assert(importer != null);
        
        using (Stream stream = provider.GetStream()) {
            if (stream is not { CanRead: true, CanSeek: true }) {
                results.Add(rid, new(BuildStatus.InvalidResourceStream));

                context = null;
                imported = null;
                return false;
            }

            try {
                context = new(options.Options);
                imported = importer.ImportObject(stream, context);

                return true;
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                
                context = null;
                imported = null;
                return false;
            }
        }
    }

    private bool Process(ResourceID rid, ContentRepresentation imported, BuildingOptions options, Processor processor, IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencies, Dictionary<ResourceID, ResourceBuildingResult> results, [NotNullWhen(true)] out ContentRepresentation? processed) {
        Debug.Assert(processor != null);
        
        try {
            processed = processor.Process(imported, new(this, options.Options, dependencies, Logger));
            return true;
        } catch (Exception e) {
            results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));

            processed = null;
            return false;
        }
    }

    private void SerializeProcessedObject(ContentRepresentation processed, ResourceID rid, BuildingOptions options) {
        var factory = SerializerFactories.GetSerializableFactory(processed.GetType())!;
        
        if (factory == null) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializerFactory, processed.GetType()));
        }

        using MemoryStream ms = new(4096);

        var serializer = factory.InternalCreateSerializer(processed, new(options.Options, Logger));
        
        CompileHelpers.Compile(serializer, ms, options.Tags);
        ms.Position = 0;
        Output.CopyCompiledResourceOutput(ms, rid);
    }

    private bool GetBuildingComponents(ResourceID rid, string importerName, [NotNullWhen(true)] out Importer? importer, string? processorName, [NotNullWhen(true)] out Processor? processor, Dictionary<ResourceID, ResourceBuildingResult> results) {
        if (!Importers.TryGetValue(importerName, out importer)) {
            results.Add(rid, new(BuildStatus.UnknownImporter));
            importer = null;
            processor = null;
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(processorName)) {
            processor = Processor.Passthrough;
            return true;
        }
        
        if (!Processors.TryGetValue(processorName, out processor)) {
            results.Add(rid, new(BuildStatus.UnknownProcessor));

            processor = null;
            return false;
        }

        return true;
    }
    
    private record VertexInfo {
        public readonly IReadOnlySet<ResourceID> DependencyIds;
        public readonly HashSet<ResourceID> DependentIds;
        public int RefCount { get; private set; }

        public bool Visited;
        public bool FaultyImport;
        public bool Cached;
        
        public ContentRepresentation? ImportOutput { get; private set; }
        public ImportingContext? ImportingContext { get; private set; }
    
        public VertexInfo(IReadOnlySet<ResourceID> dependencyIds, HashSet<ResourceID> dependentIds) {
            DependencyIds = dependencyIds;
            DependentIds = dependentIds;
            ImportOutput = null;
        }

        public void SetReferenceCount(int count) {
            Debug.Assert(RefCount == 0);
            
            RefCount = count;
        }

        public void Release() {
            if (--RefCount != 0) return;

            ImportOutput?.Dispose();
            ImportOutput = null;
            ImportingContext = null;
        }

        [MemberNotNull(nameof(ImportOutput), nameof(ImportingContext))]
        public void SetImported(ContentRepresentation importOutput, ImportingContext context) {
            Debug.Assert(ImportOutput == null);

            ImportOutput = importOutput;
            ImportingContext = context;
        }
    }
}
using Caxivitual.Lunacub.Building.Collections;
using System.Collections.Frozen;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        DateTime begin = DateTime.Now;
        Dictionary<ResourceID, ResourceBuildingResult> results = [];

        Dictionary<ResourceID, VertexInfo> vertices = BuildDependencyGraph();
        
        foreach ((_, var info) in vertices) {
            info.RefCount = info.Dependents.Count;
        }
        
        // Enumerate through root nodes
        foreach ((var rid, var info) in vertices) {
            if (info.Dependents.Count != 0) continue;
        
            BuildRootResource(vertices, rid, info, results);
        }

        foreach ((var id, var info) in vertices) {
            Console.WriteLine(id + ": " + info.State + ", " + info.RefCount);
        }

        return new(begin, DateTime.Now, results);
    }
    
    private void BuildRootResource(Dictionary<ResourceID, VertexInfo> graph, ResourceID rid, VertexInfo info, Dictionary<ResourceID, ResourceBuildingResult> results) {
        Debug.Assert(info.RefCount == 0);
        
        bool getResource = Resources.TryGetValue(rid, out var resource);
        Debug.Assert(getResource);
        
        (ResourceProvider provider, BuildingOptions options) = resource;
        DateTime resourceLastWriteTime = provider.LastWriteTime;
        
        Dictionary<ResourceID, ContentRepresentation> dependencyRepresentations = [];
        bool rebuild = false;

        try {
            if (IsResourceNeedRebuild(rid, resourceLastWriteTime, options, out _)) {
                foreach (var dependencyId in info.Dependencies) {
                    BuildDependencyResource(graph, dependencyId, results);
                    
                    if (graph[dependencyId].ContentRepresentation is not { } representation) continue;
                    dependencyRepresentations.Add(dependencyId, representation);
                }

                rebuild = true;
            } else {
                foreach (var dependencyId in info.Dependencies) {
                    BuildDependencyResource(graph, dependencyId, results);
                    rebuild |= graph[dependencyId].State == VertexState.Rebuilt;
                    
                    if (graph[dependencyId].ContentRepresentation is not { } representation) continue;
                    dependencyRepresentations.Add(dependencyId, representation);
                }
            }

            if (!rebuild) {
                results.Add(rid, new(BuildStatus.Cached));
                info.State = VertexState.Cached;
                return;
            }
            
            info.State = VertexState.Rebuilt;
            
            if (!Importers.TryGetValue(options.ImporterName, out Importer? importer)) {
                results.Add(rid, new(BuildStatus.UnknownImporter));
                return;
            }
            
            ImportingContext importingContext;
            ContentRepresentation imported;

            using Stream stream = provider.GetStream();

            if (stream is not { CanRead: true, CanSeek: true }) {
                results.Add(rid, new(BuildStatus.InvalidResourceStream));
                return;
            }
            
            try {
                importingContext = new(options.Options);
                imported = importer.ImportObject(stream, importingContext);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }
            
            Processor? processor;
            string? processorName = options.ProcessorName;
            
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

            ContentRepresentation processed;
            try {
                processed = processor.Process(imported, new(this, options.Options, dependencyRepresentations));
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
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
            IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, info.Dependencies));
            
            if (importingContext.References.Count != 0) {
                throw new NotImplementedException("Reference building is not implemented.");
            }
        } finally {
            foreach ((var dependencyId, var representation) in dependencyRepresentations) {
                var dependencyInfos = graph[dependencyId];
                
                int refcount = --dependencyInfos.RefCount;
                Debug.Assert(refcount >= 0);

                if (refcount == 0) {
                    representation.Dispose();
                    dependencyInfos.State = VertexState.Cleaned;
                }
            }
        }
    }
    
    private void BuildDependencyResource(Dictionary<ResourceID, VertexInfo> graph, ResourceID rid, Dictionary<ResourceID, ResourceBuildingResult> results) {
        bool getInfos = graph.TryGetValue(rid, out var info);
        Debug.Assert(getInfos);
        
        Debug.Assert(info!.State != VertexState.Cleaned);

        if (info.State != VertexState.Unvisited) return;
        
        bool getResource = Resources.TryGetValue(rid, out var resource);
        Debug.Assert(getResource);
        
        (ResourceProvider provider, BuildingOptions options) = resource;
        DateTime resourceLastWriteTime = provider.LastWriteTime;

        if (info.Dependencies.Count == 0) {
            if (!IsResourceNeedRebuild(rid, resourceLastWriteTime, options, out _)) {
                results.Add(rid, new(BuildStatus.Cached));
                info.State = VertexState.Cached;
                return;
            }

            info.State = VertexState.Rebuilt;
            
            if (!Importers.TryGetValue(options.ImporterName, out Importer? importer)) {
                results.Add(rid, new(BuildStatus.UnknownImporter));
                return;
            }
            
            ImportingContext importingContext;
            ContentRepresentation imported;

            using Stream stream = provider.GetStream();

            if (stream is not { CanRead: true, CanSeek: true }) {
                results.Add(rid, new(BuildStatus.InvalidResourceStream));
                return;
            }
            
            try {
                importingContext = new(options.Options);
                imported = importer.ImportObject(stream, importingContext);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }

            info.ContentRepresentation = imported;
            
            Processor? processor;
            string? processorName = options.ProcessorName;
            
            if (string.IsNullOrWhiteSpace(processorName)) {
                processor = Processor.Passthrough;
            } else if (!Processors.TryGetValue(processorName, out processor)) {
                results.Add(rid, new(BuildStatus.UnknownProcessor));
                return;
            }
            
            ContentRepresentation processed;
            try {
                processed = processor.Process(imported, new(this, options.Options, FrozenDictionary<ResourceID, ContentRepresentation>.Empty));
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
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
            IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, info.Dependencies));
            
            if (importingContext.References.Count != 0) {
                throw new NotImplementedException("Reference building is not implemented.");
            }

            return;
        }
        
        // Got dependencies, walkthrough more troublesome route.
        Dictionary<ResourceID, ContentRepresentation> dependencyRepresentations = [];
        bool rebuild = false;

        try {
            if (IsResourceNeedRebuild(rid, resourceLastWriteTime, options, out _)) {
                foreach (var dependencyId in info.Dependencies) {
                    BuildDependencyResource(graph, dependencyId, results);
                    
                    if (graph[dependencyId].ContentRepresentation is not { } representation) continue;
                    dependencyRepresentations.Add(dependencyId, representation);
                }

                rebuild = true;
            } else {
                foreach (var dependencyId in info.Dependencies) {
                    BuildDependencyResource(graph, dependencyId, results);
                    rebuild |= graph[dependencyId].State == VertexState.Rebuilt;
                    
                    if (graph[dependencyId].ContentRepresentation is not { } representation) continue;
                    dependencyRepresentations.Add(dependencyId, representation);
                }
            }

            if (!rebuild) {
                results.Add(rid, new(BuildStatus.Cached));
                info.State = VertexState.Cached;
                return;
            }
            
            info.State = VertexState.Rebuilt;
            
            if (!Importers.TryGetValue(options.ImporterName, out Importer? importer)) {
                results.Add(rid, new(BuildStatus.UnknownImporter));
                return;
            }
            
            ImportingContext importingContext;
            ContentRepresentation imported;

            using Stream stream = provider.GetStream();

            if (stream is not { CanRead: true, CanSeek: true }) {
                results.Add(rid, new(BuildStatus.InvalidResourceStream));
                return;
            }
            
            try {
                importingContext = new(options.Options);
                imported = importer.ImportObject(stream, importingContext);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }
            
            info.ContentRepresentation = imported;
            
            Processor? processor;
            string? processorName = options.ProcessorName;
            
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

            ContentRepresentation processed;
            try {
                processed = processor.Process(imported, new(this, options.Options, dependencyRepresentations));
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
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
            IncrementalInfos.Add(rid, new(resourceLastWriteTime, options, info.Dependencies));
            
            if (importingContext.References.Count != 0) {
                throw new NotImplementedException("Reference building is not implemented.");
            }
        } finally {
            Console.WriteLine("Dispose B: " + dependencyRepresentations.Count);
            
            foreach ((var dependencyId, var representation) in dependencyRepresentations) {
                var dependencyInfos = graph[dependencyId];
                
                int refcount = --dependencyInfos.RefCount;
                Debug.Assert(refcount >= 0);

                if (refcount == 0) {
                    representation.Dispose();
                    dependencyInfos.State = VertexState.Cleaned;
                }
            }
        }
        
        // bool rebuild = false;
        // if (IsResourceNeedRebuild(rid, resourceLastWriteTime, options, out _)) {
        //     foreach (var dependency in info!.Dependencies) {
        //         BuildDependencyResource(graph, rid, results);
        //             
        //         if (graph[dependency].ContentRepresentation is not { } representation) continue;
        //         dependencyRepresentations.Add(dependency, representation);
        //     }
        //
        //     rebuild = true;
        // } else {
        //     foreach (var dependency in info!.Dependencies) {
        //         BuildDependencyResource(graph, rid, results);
        //         rebuild |= graph[dependency].Rebuilt;
        //             
        //         if (graph[dependency].ContentRepresentation is not { } representation) continue;
        //         dependencyRepresentations.Add(dependency, representation);
        //     }
        // }
        
        // if (IsResourceNeedRebuild(rid, provider.LastWriteTime, options, out var incrementalInfo)) {
        //     graph[rid].Rebuilt = true;
        // } else {
        //     graph[rid].Rebuilt = false;
        // }
    }
    
    private Dictionary<ResourceID, VertexInfo> BuildDependencyGraph() {
        Dictionary<ResourceID, VertexInfo> outputVertices = [];
        
        // Collecting edges.
        foreach ((var rid, var resource) in Resources) {
            CollectRecursively(outputVertices, rid, resource);
        }

        ValidateGraph(outputVertices);
        
        // Can we go further by ignore the resources that can be cached and go upward?
        
        return outputVertices;

        void CollectRecursively(Dictionary<ResourceID, VertexInfo> receiver, ResourceID rid, BuildingResource resource) {
            if (receiver.ContainsKey(rid)) return;

            if (!Importers.TryGetValue(resource.Options.ImporterName, out Importer? importer)) {
                throw new ArgumentException($"Failed to get importer '{resource.Options.ImporterName}' for resource '{rid}'.");
            }
            
            using var stream = resource.Provider.GetStream();
            var dependencies = importer.GetDependencies(stream).Where(FilterContainsResource).ToHashSet();

            if (dependencies.Count == 0) return;
            
            receiver.Add(rid, new(dependencies, []));

            foreach (var dependency in dependencies) {
                CollectRecursivelyWithDependent(receiver, dependency, rid);
            }
        }
        
        void CollectRecursivelyWithDependent(Dictionary<ResourceID, VertexInfo> receiver, ResourceID rid, ResourceID dependent) {
            if (receiver.TryGetValue(rid, out var edges)) {
                edges.Dependents.Add(dependent);
            } else {
                if (!Resources.TryGetValue(rid, out var resource)) return;
                
                if (!Importers.TryGetValue(resource.Options.ImporterName, out Importer? importer)) {
                    throw new ArgumentException($"Failed to get importer '{resource.Options.ImporterName}' for resource '{rid}'.");
                }

                using var stream = resource.Provider.GetStream();
                var dependencies = importer.GetDependencies(stream).Where(FilterContainsResource).ToHashSet();

                receiver.Add(rid, new(dependencies, [dependent]));
                
                foreach (var dependency in dependencies) {
                    CollectRecursivelyWithDependent(receiver, dependency, rid);
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
                    throw new InvalidOperationException($"Cyclic dependency detected: {string.Join(" -> ", path.Reverse())}.");
                }
                
                foreach (var dependencyID in vertices[rid].Dependencies) {
                    Visit(dependencyID, vertices, temporaryMark, permanentMark, path);
                }
                
                permanentMark.Add(rid);
                path.Pop();
            }
        }
    }
    
    private bool IsResourceNeedRebuild(ResourceID rid, DateTime resourceLastWriteTime, BuildingOptions currentOptions, out IncrementalInfo previousIncrementalInfo) {
        // If resource has been built before, and have old report, we can begin checking for caching.
        if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && IncrementalInfos.TryGet(rid, out previousIncrementalInfo)) {
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousIncrementalInfo.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
                // If the options are equal, no need to rebuild.
                if (currentOptions.Equals(previousIncrementalInfo.Options)) {
                    return false;
                }
            }

            return true;
        }

        previousIncrementalInfo = default;
        return true;
    }

    private bool ImportAndProcess(Dictionary<ResourceID, ResourceBuildingResult> results, Stream stream, ResourceID rid, BuildingOptions options, Importer importer, IReadOnlyDictionary<ResourceID, ContentRepresentation> dependencyRepresentations, ImportingContext importingContext, [NotNullWhen(true)] out ContentRepresentation? processed) {
        using (ContentRepresentation imported = importer.ImportObject(stream, importingContext)) {
            string? processorName = options.ProcessorName;
        
            Processor? processor;
        
            if (string.IsNullOrWhiteSpace(processorName)) {
                processor = Processor.Passthrough;
            } else if (!Processors.TryGetValue(processorName, out processor)) {
                results.Add(rid, new(BuildStatus.UnknownProcessor));
                
                processed = null;
                return false;
            }
        
            if (!processor.CanProcess(imported)) {
                results.Add(rid, new(BuildStatus.CannotProcess));
                
                processed = null;
                return false;
            }
        
            try {
                processed = processor.Process(imported, new(this, options.Options, dependencyRepresentations));
                return true;
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                
                processed = null;
                return false;
            }
        }
    }

    private void SerializeProcessedObject(ContentRepresentation processed, ResourceID rid, BuildingOptions options) {
        if (SerializerFactories.GetSerializableFactory(processed.GetType()) is not { } factory) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializer, processed.GetType()));
        }
                
        using MemoryStream ms = new(4096);
        CompileHelpers.Compile(factory.InternalCreateSerializer(processed, new(options.Options)), ms, options.Tags);
        ms.Position = 0;
        Output.CopyCompiledResourceOutput(ms, rid);
    }

    private record VertexInfo {
        public readonly IReadOnlySet<ResourceID> Dependencies;
        public readonly HashSet<ResourceID> Dependents;
        public int RefCount;
        public ContentRepresentation? ContentRepresentation;
        public VertexState State;

        public VertexInfo(IReadOnlySet<ResourceID> dependencies, HashSet<ResourceID> dependents) {
            Dependencies = dependencies;
            Dependents = dependents;
            ContentRepresentation = null;
        }
    }

    private enum VertexState {
        Unvisited = 0,
        Cached = 1,
        Rebuilt = 2,
        Cleaned = 3,
        // Failed = 4,
    }
}
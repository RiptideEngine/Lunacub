using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        try {
            RecursionTracer tracer = new();
            Dictionary<ResourceID, BuildingReport> reports = [];
            
            foreach ((var rid, _) in Resources) {
                bool get = Resources.TryGet(rid, out string? path, out BuildingOptions options);
                Debug.Assert(get);
                
                tracer.Tempmark.Clear();
                tracer.CyclePath.Clear();
                BuildResource(rid, path!, options, true, reports, in tracer, null);
            }

            return new(reports, null);
        } catch (Exception e) {
            return new(null, e);
        }
    }

    public BuildingResult BuildResource(ResourceID rid) {
        if (!Resources.TryGet(rid, out string? resourcePath, out BuildingOptions buildingOptions)) {
            return new(null, new ArgumentException($"ResourceID '{rid}' does not exist."));
        }
        
        try {
            RecursionTracer tracer = new();
            Dictionary<ResourceID, BuildingReport> reports = [];
            
            Recursion(rid, resourcePath, buildingOptions, reports, in tracer);
            
            return new(reports, null);
        } catch (Exception e) {
            return new(null, e);
        }

        void Recursion(ResourceID rid, string resourcePath, BuildingOptions buildingOptions, Dictionary<ResourceID, BuildingReport> reports, in RecursionTracer tracer) {
            HashSet<ResourceID> requestingBuilds = [];
            
            BuildResource(rid, resourcePath, buildingOptions, true, reports, in tracer, requestingBuilds);

            foreach (var requesting in requestingBuilds) {
                if (!Resources.TryGet(requesting, out string? path, out BuildingOptions options)) continue;
                
                Recursion(requesting, path, options, reports, in tracer);
            }
        }
    }

    private bool BuildResource(ResourceID rid, string resourcePath, BuildingOptions options, bool throwExceptionOnCycle, Dictionary<ResourceID, BuildingReport> reports, in RecursionTracer tracer, HashSet<ResourceID>? requestingBuilds) {
        if (tracer.Permark.Contains(rid)) return false;

        using (tracer.PushPath(rid)) {
            if (!tracer.Tempmark.Add(rid)) {
                if (throwExceptionOnCycle) throw new ArgumentException($"Cycle detected ({string.Join(" -> ", tracer.CyclePath.Reverse())}).");

                return false;
            }

            try {
                // If resource has been built before, and have old report, we can begin checking for caching.
                if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && _reportTracker.TryGetReport(rid, out var previousReport)) {
                    DateTime resourceLastWriteTime = File.GetLastWriteTime(resourcePath);

                    // Check if resource's last write time is the same as the time stored in report.
                    // Check if destination's last write time is later than resource's last write time.
                    if (resourceLastWriteTime == previousReport.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
                        // If any importing option is different.
                        if (CompareOptions(options, previousReport.Options)) {
                            // Valid cache.
                            bool isDependencyRebuilt = false;

                            if (previousReport.Dependencies is { } dependencies) {
                                foreach (var dependencyRid in dependencies) {
                                    if (!Resources.TryGet(dependencyRid, out string? dependencyResourcePath, out BuildingOptions dependencyBuildingOptions)) {
                                        isDependencyRebuilt = true;
                                        continue;
                                    }
                                    
                                    isDependencyRebuilt |= BuildResource(dependencyRid, dependencyResourcePath, dependencyBuildingOptions, true, reports, in tracer, requestingBuilds);
                                }
                            }

                            if (!isDependencyRebuilt) return false;
                        }
                    }
                }

                if (!Importers.TryGetValue(options.ImporterName, out Importer? importer)) {
                    throw new ArgumentException(string.Format(ExceptionMessages.UnregisteredImporter, options.ImporterName));
                }

                ContentRepresentation imported;
                ImportingContext context;

                using (FileStream stream = File.OpenRead(resourcePath!)) {
                    context = new();
                    imported = importer.ImportObject(stream, context);
                }

                try {
                    HashSet<ResourceID> dependencies = [];
                    foreach (var reference in context.References) {
                        if (reference.Type != ResourceReferenceType.Dependency) continue;

                        if (!Resources.TryGet(reference.Rid, out string? dependencyResourcePath, out BuildingOptions dependencyBuildingOptions)) continue;

                        BuildResource(reference.Rid, dependencyResourcePath, dependencyBuildingOptions, true, reports, in tracer, requestingBuilds);
                        dependencies.Add(reference.Rid);
                    }

                    string? processorName = options.ProcessorName;

                    Processor? processor;

                    if (string.IsNullOrWhiteSpace(processorName)) {
                        processor = Processor.Passthrough;
                    } else if (!Processors.TryGetValue(processorName, out processor)) {
                        throw new ArgumentException(string.Format(ExceptionMessages.UnregisteredProcessor, processorName));
                    }
                    
                    if (!processor.CanProcess(imported)) {
                        throw new InvalidOperationException(
                            $"Processor key '{processorName}' type '{processor.GetType().FullName}' doesn't allow processing object of type '{imported.GetType().FullName}'.");
                    }
                    
                    BuildingReport report = new() {
                        Dependencies = dependencies,
                        Options = options,
                        SourceLastWriteTime = File.GetLastWriteTime(resourcePath!),
                    };
                    
                    ContentRepresentation processed = processor.Process(imported!);
                    try {
                        CompileObject(processed, rid);
                    } finally {
                        processor.DisposeObject(processed);
                    }

                    reports.Add(rid, report);
                    _reportTracker.AddPendingReport(rid, report);

                    if (requestingBuilds != null) {
                        foreach (var reference in context.References) {
                            if (reference.Type == ResourceReferenceType.Reference) continue;

                            if (!Resources.Contains(reference.Rid)) continue;

                            requestingBuilds.Add(reference.Rid);
                        }
                    }
                } finally {
                    importer.DisposeObject(imported);
                }

                return true;
            } finally {
                requestingBuilds?.Remove(rid);
                tracer.Permark.Add(rid);
            }
        }

        static bool CompareOptions(BuildingOptions currentOptions, BuildingOptions previousOptions) {
            if (currentOptions.ImporterName != previousOptions.ImporterName) return false;
            if (currentOptions.ProcessorName != previousOptions.ProcessorName) return false;

            // TODO: Compare difference in import configurations.
        
            return true;
        }
    }

    private void CompileObject(ContentRepresentation processed, ResourceID rid) {
        if (Serializers.GetSerializable(processed.GetType()) is not { } serializer) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializer, processed.GetType()));
        }
        
        using Stream outputStream = Output.CreateDestinationStream(rid);
        outputStream.SetLength(0);
        outputStream.Flush();

        using BinaryWriter bwriter = new(outputStream);
        bwriter.Write(CompilingConstants.MagicIdentifier);
        bwriter.Write((ushort)1);
        bwriter.Write((ushort)0);

        const int NumChunks = 2;
        
        bwriter.Write(NumChunks);
        
        long chunkLocationPosition = bwriter.BaseStream.Position;
        Span<KeyValuePair<uint, int>> chunks = stackalloc KeyValuePair<uint, int>[NumChunks];
        int chunkIndex = 0;

        bwriter.Seek(8 * NumChunks, SeekOrigin.Current);
        
        try {
            int chunkStart = (int)outputStream.Position;
            WriteDataChunk(bwriter, processed, serializer);
            chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ResourceDataChunkTag), chunkStart);

            chunkStart = (int)outputStream.Position;
            WriteDeserializerChunk(bwriter, serializer);
            chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.DeserializationChunkTag), chunkStart);
        } finally {
            Debug.Assert(chunkIndex == NumChunks);
            
            outputStream.Seek(chunkLocationPosition, SeekOrigin.Begin);
            
            foreach ((uint chunkIdentifier, int chunkSize) in chunks) {
                bwriter.Write(chunkIdentifier);
                bwriter.Write(chunkSize);
            }
        }

        static void WriteDataChunk(BinaryWriter writer, ContentRepresentation obj, Serializer serializer) {
            Stream outputStream = writer.BaseStream;
            
            writer.Write("DATA"u8);
            {
                var chunkLenPosition = (int)outputStream.Position;
            
                writer.Seek(4, SeekOrigin.Current);
                serializer.SerializeObject(obj, outputStream);
                
                var serializedSize = (int)(outputStream.Position - chunkLenPosition - 4);
            
                writer.Seek(chunkLenPosition, SeekOrigin.Begin);
                writer.Write(serializedSize);
                
                writer.Seek(chunkLenPosition, SeekOrigin.Current);
            }
        }

        static void WriteDeserializerChunk(BinaryWriter writer, Serializer serializer) {
            Stream outputStream = writer.BaseStream;
            
            writer.Write("DESR"u8);
            {
                var chunkLenPosition = (int)outputStream.Position;
            
                writer.Seek(4, SeekOrigin.Current);
                
                writer.Write(serializer.DeserializerName);
                
                var serializedSize = (int)(outputStream.Position - chunkLenPosition - 4);
            
                writer.Seek(chunkLenPosition, SeekOrigin.Begin);
                writer.Write(serializedSize);
                
                writer.Seek(chunkLenPosition, SeekOrigin.Current);
            }
        }
    }

    private readonly struct RecursionTracer {
        public readonly HashSet<ResourceID> Permark;
        public readonly HashSet<ResourceID> Tempmark;
        public readonly Stack<ResourceID> CyclePath;

        public RecursionTracer() {
            Permark = [];
            Tempmark = [];
            CyclePath = [];
        }

        public PathScope PushPath(ResourceID rid) => new(rid, CyclePath);

        public readonly struct PathScope : IDisposable {
            private readonly Stack<ResourceID> Stack;
            
            public PathScope(ResourceID rid, Stack<ResourceID> stack) {
                stack.Push(rid);
                Stack = stack;
            }
            
            public void Dispose() {
                Stack.Pop();
            }
        }
    }
}
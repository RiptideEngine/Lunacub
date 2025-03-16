using System.Buffers.Binary;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        try {
            HashSet<ResourceID> permark = [];
            HashSet<ResourceID> tempmark = [];
            Stack<ResourceID> cyclePath = [];

            Dictionary<ResourceID, BuildingReport> reports = [];
            
            foreach ((var rid, _) in Resources) {
                tempmark.Clear();
                cyclePath.Clear();
                BuildResource(rid, reports, permark, tempmark, cyclePath);
            }

            return new(reports, null);
        } catch (Exception e) {
            return new(null, e);
        }
    }

    private bool BuildResource(ResourceID rid, Dictionary<ResourceID, BuildingReport> reports, HashSet<ResourceID> permark, HashSet<ResourceID> tempmark, Stack<ResourceID> cyclePath) {
        if (permark.Contains(rid)) return false;
        
        bool get = Resources.TryGet(rid, out string? resourcePath, out BuildingOptions options);
        Debug.Assert(get);
        
        cyclePath.Push(rid);

        if (!tempmark.Add(rid)) {
            throw new ArgumentException($"Cycle detected ({string.Join(" -> ", cyclePath.Reverse())}).");
        }

        string buildDestination = Output.GetBuildDestination(rid);

        // If resource has been built before, and have old report, we can begin checking for caching.
        if (File.Exists(buildDestination) && _reportTracker.TryGetReport(rid, out var previousReport)) {
            DateTime resourceLastWriteTime = File.GetLastWriteTime(resourcePath!);
            
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousReport.SourceLastWriteTime && File.GetLastWriteTime(buildDestination) > resourceLastWriteTime) {
                // If any importing option is different.
                if (CompareOptions(options, previousReport.Options)) {
                    // Valid cache.
                    bool isDependencyRebuilt = false;
                    
                    if (previousReport.Dependencies is { } dependencies) {
                        foreach (var dependency in dependencies) {
                            isDependencyRebuilt |= BuildResource(dependency, reports, permark, tempmark, cyclePath);
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
            if (context.Dependencies is { } dependencies) {
                foreach (var dependency in dependencies) {
                    BuildResource(dependency, reports, permark, tempmark, cyclePath);
                }
            }
            
            string? processorName = options.ProcessorName;
            
            BuildingReport report;
            ResourceReference resourceRef = new(rid, resourcePath!);
            
            if (string.IsNullOrWhiteSpace(processorName)) {
                report = new() {
                    Dependencies = context.Dependencies,
                    DestinationPath = buildDestination,
                    Options = options,
                    SourceLastWriteTime = File.GetLastWriteTime(resourcePath!),
                };
                
                CompileObject(imported, resourceRef);
            } else {
                if (!Processors.TryGetValue(processorName, out var processor)) {
                    throw new ArgumentException(string.Format(ExceptionMessages.UnregisteredProcessor, processorName));
                }
            
                if (!processor.CanProcess(imported)) {
                    throw new InvalidOperationException($"Processor key '{processorName}' type '{processor.GetType().FullName}' doesn't allow processing object of type '{imported.GetType().FullName}'.");
                }
                
                report = new() {
                    Dependencies = context.Dependencies,
                    DestinationPath = buildDestination,
                    Options = options,
                    SourceLastWriteTime = File.GetLastWriteTime(resourcePath!),
                };

                ContentRepresentation processed = processor.Process(imported!);

                try {
                    CompileObject(processed, resourceRef);
                } finally {
                    processor.DisposeObject(processed);
                }
            }
            
            reports.Add(rid, report);
            _reportTracker.AddPendingReport(rid, report);
        } finally {
            importer.DisposeObject(imported);
        }

        permark.Add(rid);
        cyclePath.Pop();

        return true;
        
        static bool CompareOptions(BuildingOptions currentOptions, BuildingOptions previousOptions) {
            if (currentOptions.ImporterName != previousOptions.ImporterName) return false;
            if (currentOptions.ProcessorName != previousOptions.ProcessorName) return false;

            // TODO: Compare difference in import configurations.
        
            return true;
        }
    }

    private void CompileObject(ContentRepresentation processed, ResourceReference reference) {
        if (Serializers.GetSerializable(processed.GetType()) is not { } serializer) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializer, processed.GetType()));
        }
        
        using Stream outputStream = Output.CreateDestinationStream(reference);
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
}
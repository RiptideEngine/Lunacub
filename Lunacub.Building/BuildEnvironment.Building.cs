using System.Buffers.Binary;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        DateTime start = DateTime.Now;

        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        foreach ((var rid, _) in Resources) {
            bool get = Resources.TryGet(rid, out string? path, out BuildingOptions options);
            Debug.Assert(get);

            BuildResource(rid, path!, options, results);
        }

        return new(start, DateTime.Now, results);
    }

    public BuildingResult BuildResource(ResourceID rid) {
        DateTime start = DateTime.Now;
        
        if (!Resources.TryGet(rid, out string? resourcePath, out BuildingOptions buildingOptions)) {
            return new(start, start, new Dictionary<ResourceID, ResourceBuildingResult> {
                [rid] = new(BuildStatus.ResourceNotFound),
            });
        }

        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        BuildResource(rid, resourcePath, buildingOptions, results);
        
        return new(start, DateTime.Now, results);
    }

    private void BuildResource(ResourceID rid, string resourcePath, BuildingOptions options, Dictionary<ResourceID, ResourceBuildingResult> results) {
        if (results.ContainsKey(rid)) return;
        
        DateTime resourceLastWriteTime = File.GetLastWriteTime(resourcePath);

        // If resource has been built before, and have old report, we can begin checking for caching.
        if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && _incrementalInfoStorage.TryGet(rid, out var previousReport)) {
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

        ContentRepresentation imported;
        ImportingContext context;

        try {
            using FileStream stream = File.OpenRead(resourcePath);

            context = new();
            imported = importer.ImportObject(stream, context);
        } catch (Exception e) {
            results.Add(rid, new(BuildStatus.ImportingFailed, ExceptionDispatchInfo.Capture(e)));
            return;
        }

        try {
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
                processed = processor.Process(imported);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.ProcessingFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            }

            try {
                using MemoryStream ms = new(1024);
                CompileObject(ms, processed);
                ms.Position = 0;
                Output.CopyCompiledResourceOutput(ms, rid);
            } catch (Exception e) {
                results.Add(rid, new(BuildStatus.CompilationFailed, ExceptionDispatchInfo.Capture(e)));
                return;
            } finally {
                processor.DisposeObject(processed);
            }

            results.Add(rid, new(BuildStatus.Success));
            _incrementalInfoStorage.Add(rid, incrementalInfo);
        } finally {
            importer.DisposeObject(imported);
        }
        
        foreach (var reference in context.References) {
            if (!Resources.TryGet(reference, out string? refResourcePath, out BuildingOptions refResourceBuildingOptions)) continue;

            BuildResource(reference, refResourcePath, refResourceBuildingOptions, results);
        }
    }
    
    private void CompileObject(Stream outputStream, ContentRepresentation processed) {
        if (Serializers.GetSerializable(processed.GetType()) is not { } serializer) {
            throw new InvalidOperationException(string.Format(ExceptionMessages.NoSuitableSerializer, processed.GetType()));
        }

        using BinaryWriter bwriter = new(outputStream, Encoding.UTF8, true);
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
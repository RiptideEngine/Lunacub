using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    public BuildingResult BuildResources() {
        DateTime start = DateTime.Now;

        HashSet<ResourceID> visited = [];
        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        foreach ((var rid, _) in Resources) {
            bool get = Resources.TryGet(rid, out string? path, out BuildingOptions options);
            Debug.Assert(get);
            
            visited.Clear();
            BuildResource(rid, path!, options, results, visited);
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

        HashSet<ResourceID> visited = [];
        Dictionary<ResourceID, ResourceBuildingResult> results = [];
        
        BuildResource(rid, resourcePath, buildingOptions, results, visited);
        
        return new(start, DateTime.Now, results);
    }

    /// <summary>
    /// The main building function, will be called recursively (Depth-First Traversing).
    /// </summary>
    /// <param name="rid">ResourceID of currently importing resource.</param>
    /// <param name="resourcePath">The file path of the currently importing resource.</param>
    /// <param name="options">Importing options of currently importing resource.</param>
    /// <param name="results">Resource Resource Dictionary to receive the result.</param>
    /// <param name="tracer">Tracer that contains the objects needed for Depth-First Traversing.</param>
    /// <param name="requestingBuilds">Dictionary that receive the requested building resources.</param>
    /// <param name="rebuilt"><see langword="true"/> if resource is rebuilt instead of cached, <see langword="false"/> otherwise.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private void BuildResource(ResourceID rid, string resourcePath, BuildingOptions options, Dictionary<ResourceID, ResourceBuildingResult> results, HashSet<ResourceID> visited) {
        if (!visited.Add(rid)) return;
        
        DateTime resourceLastWriteTime = File.GetLastWriteTime(resourcePath);

        // If resource has been built before, and have old report, we can begin checking for caching.
        if (Output.GetResourceLastBuildTime(rid) is { } resourceLastBuildTime && _incrementalInfoStorage.TryGet(rid, out var previousReport)) {
            // Check if resource's last write time is the same as the time stored in report.
            // Check if destination's last write time is later than resource's last write time.
            if (resourceLastWriteTime == previousReport.SourceLastWriteTime && resourceLastBuildTime > resourceLastWriteTime) {
                // If any importing option is different.
                if (AreOptionsEqual(options, previousReport.Options)) {
                    ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(results, rid, out bool exists);

                    if (!exists) {
                        reference = new(BuildStatus.Cached);
                    }
                    
                    return;
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
            string? processorName = options.ProcessorName;

            Processor? processor;

            if (string.IsNullOrWhiteSpace(processorName)) {
                processor = Processor.Passthrough;
            } else if (!Processors.TryGetValue(processorName, out processor)) {
                throw new ArgumentException(string.Format(ExceptionMessages.UnregisteredProcessor, processorName));
            }

            if (!processor.CanProcess(imported)) {
                throw new InvalidOperationException($"Processor key '{processorName}' type '{processor.GetType().FullName}' doesn't allow processing object of type '{imported.GetType().FullName}'.");
            }

            IncrementalInfo incrementalInfo = new(resourceLastWriteTime, options);

            ContentRepresentation processed = processor.Process(imported!);

            try {
                CompileObject(processed, rid);
            } finally {
                processor.DisposeObject(processed);
            }

            results.Add(rid, new(BuildStatus.Success));
            _incrementalInfoStorage.Add(rid, incrementalInfo);

            foreach (var reference in context.References) {
                if (!Resources.TryGet(reference, out string? refResourcePath, out BuildingOptions refResourceBuildingOptions)) continue;

                BuildResource(reference, refResourcePath, refResourceBuildingOptions, results, visited);
            }
        } finally {
            importer.DisposeObject(imported);
        }
    }
    
    private static bool AreOptionsEqual(BuildingOptions currentOptions, BuildingOptions previousOptions) {
        if (currentOptions.ImporterName != previousOptions.ImporterName) return false;
        if (currentOptions.ProcessorName != previousOptions.ProcessorName) return false;

        // TODO: Compare difference in import configurations.
        
        return true;
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
}
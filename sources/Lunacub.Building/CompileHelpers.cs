using Microsoft.IO;

namespace Caxivitual.Lunacub.Building;

internal static class CompileHelpers {
    public static unsafe void Compile(BuildEnvironment environment, Serializer serializer, Stream outputStream, IReadOnlyCollection<string> tags) {
        using BinaryWriter bwriter = new(outputStream, Encoding.UTF8, true);
        bwriter.Write(CompilingConstants.MagicIdentifier);
        bwriter.Write((ushort)1);
        bwriter.Write((ushort)0);
        
        bwriter.Write(4);
        
        long chunkLocationPosition = bwriter.BaseStream.Position;
        Span<KeyValuePair<uint, int>> chunks = stackalloc KeyValuePair<uint, int>[4];
        int chunkIndex = 0;

        bwriter.Seek(sizeof(KeyValuePair<uint, int>) * chunks.Length, SeekOrigin.Current);
        
        int chunkStart = (int)outputStream.Position;
        WriteTagChunk(bwriter, tags);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.TagChunkTag), chunkStart);
        
        chunkStart = (int)outputStream.Position;
        WriteDataChunk(environment.MemoryStreamManager, bwriter, serializer);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ResourceDataChunkTag), chunkStart);

        chunkStart = (int)outputStream.Position;
        WriteOptionsChunk(environment.MemoryStreamManager, bwriter, serializer);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ImportOptionsChunkTag), chunkStart);
        
        chunkStart = (int)outputStream.Position;
        WriteDeserializerChunk(bwriter, serializer);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.DeserializationChunkTag), chunkStart);
        
        outputStream.Seek(chunkLocationPosition, SeekOrigin.Begin);
        
        foreach ((uint chunkIdentifier, int chunkSize) in chunks) {
            bwriter.Write(chunkIdentifier);
            bwriter.Write(chunkSize);
        }
    }

    private static void WriteTagChunk(BinaryWriter writer, IReadOnlyCollection<string> tags) {
        Stream outputStream = writer.BaseStream;
        
        writer.Write(CompilingConstants.TagChunkTag);
        {
            var chunkLenPosition = (int)outputStream.Position;
            writer.Seek(4, SeekOrigin.Current);
            
            writer.Write(tags.Count);

            foreach (string tag in tags) {
                writer.Write(tag);
            }
            
            var contentSize = (int)(outputStream.Position - chunkLenPosition - 4);
        
            writer.Seek(chunkLenPosition, SeekOrigin.Begin);
            writer.Write(contentSize);
            
            writer.Seek(contentSize, SeekOrigin.Current);
        }
    }
    
    private static void WriteDataChunk(RecyclableMemoryStreamManager memoryStreamManager, BinaryWriter writer, Serializer serializer) {
        using (var dataStream = memoryStreamManager.GetStream("DataStream", 0, false)) {
            serializer.SerializeObject(dataStream);
            
            writer.Write(CompilingConstants.ResourceDataChunkTag);
            {
                writer.Write((int)dataStream.Length);
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.CopyTo(writer.BaseStream);
            }
        }
    }
    
    private static void WriteOptionsChunk(RecyclableMemoryStreamManager memoryStreamManager, BinaryWriter writer, Serializer serializer) {
        using (var optionStream = memoryStreamManager.GetStream("OptionStream", 0, false)) {
            serializer.SerializeObject(optionStream);
            
            writer.Write(CompilingConstants.ResourceDataChunkTag);
            {
                writer.Write((int)optionStream.Length);
                optionStream.Seek(0, SeekOrigin.Begin);
                optionStream.CopyTo(writer.BaseStream);
            }
        }
    }

    private static void WriteDeserializerChunk(BinaryWriter writer, Serializer serializer) {
        Stream outputStream = writer.BaseStream;

        writer.Write(CompilingConstants.DeserializationChunkTag); {
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
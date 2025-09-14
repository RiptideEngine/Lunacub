using Microsoft.IO;

namespace Caxivitual.Lunacub.Building;

internal static class CompileHelpers {
    public static unsafe void Compile(BuildEnvironment environment, Serializer serializer, Stream outputStream, IReadOnlyCollection<string> tags) {
        using BinaryWriter bwriter = new(outputStream, Encoding.UTF8, true);
        bwriter.Write(CompilingConstants.MagicIdentifier.AsSpan);
        bwriter.Write((ushort)1);
        bwriter.Write((ushort)0);
        
        // TODO: Rewrite this into a system instead of hard-coded values.
        bwriter.Write(3);
        
        long chunkLocationPosition = bwriter.BaseStream.Position;
        Span<ChunkOffset> chunks = stackalloc ChunkOffset[3];
        int chunkIndex = 0;

        bwriter.Seek(sizeof(ChunkOffset) * chunks.Length, SeekOrigin.Current);
        
        uint chunkStart = (uint)outputStream.Position;
        WriteDataChunk(environment.MemoryStreamManager, bwriter, serializer);
        chunks[chunkIndex++] = new(CompilingConstants.ResourceDataChunkTag, chunkStart);

        chunkStart = (uint)outputStream.Position;
        WriteOptionsChunk(environment.MemoryStreamManager, bwriter, serializer);
        chunks[chunkIndex++] = new(CompilingConstants.ImportOptionsChunkTag, chunkStart);
        
        chunkStart = (uint)outputStream.Position;
        WriteDeserializerChunk(bwriter, serializer);
        chunks[chunkIndex++] = new(CompilingConstants.DeserializationChunkTag, chunkStart);
        
        outputStream.Seek(chunkLocationPosition, SeekOrigin.Begin);
        
        foreach (ChunkOffset offset in chunks) {
            bwriter.Write(offset.Tag.AsSpan);
            bwriter.Write(offset.Offset);
        }
    }
    
    private static void WriteDataChunk(RecyclableMemoryStreamManager memoryStreamManager, BinaryWriter writer, Serializer serializer) {
        using (var dataStream = memoryStreamManager.GetStream("DataStream", 0, false)) {
            serializer.SerializeObject(dataStream);
            
            writer.Write(CompilingConstants.ResourceDataChunkTag.AsSpan);
            {
                writer.Write((int)dataStream.Length);
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.CopyTo(writer.BaseStream);
            }
        }
    }
    
    private static void WriteOptionsChunk(RecyclableMemoryStreamManager memoryStreamManager, BinaryWriter writer, Serializer serializer) {
        using (var optionStream = memoryStreamManager.GetStream("OptionStream", 0, false)) {
            serializer.SerializeOptions(optionStream);
            
            writer.Write(CompilingConstants.ImportOptionsChunkTag.AsSpan);
            {
                writer.Write((int)optionStream.Length);
                optionStream.Seek(0, SeekOrigin.Begin);
                optionStream.CopyTo(writer.BaseStream);
            }
        }
    }

    private static void WriteDeserializerChunk(BinaryWriter writer, Serializer serializer) {
        Stream outputStream = writer.BaseStream;

        writer.Write(CompilingConstants.DeserializationChunkTag.AsSpan);
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
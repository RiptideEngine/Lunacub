using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Building;

internal static unsafe class CompileHelpers {
    public static void Compile(Serializer serializer, Stream outputStream) {
        using BinaryWriter bwriter = new(outputStream, Encoding.UTF8, true);
        bwriter.Write(CompilingConstants.MagicIdentifier);
        bwriter.Write((ushort)1);
        bwriter.Write((ushort)0);

        bwriter.Write(3);
        
        long chunkLocationPosition = bwriter.BaseStream.Position;
        Span<KeyValuePair<uint, int>> chunks = stackalloc KeyValuePair<uint, int>[3];
        int chunkIndex = 0;

        bwriter.Seek(sizeof(KeyValuePair<uint, int>) * 3, SeekOrigin.Current);
        
        int chunkStart = (int)outputStream.Position;
        WriteDataChunk(bwriter, serializer);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ResourceDataChunkTag), chunkStart);

        chunkStart = (int)outputStream.Position;
        WriteOptionsChunk(bwriter, serializer);
        chunks[chunkIndex++] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ImportOptionsChunkTag), chunkStart);
        
        chunkStart = (int)outputStream.Position;
        WriteDeserializerChunk(bwriter, serializer);
        chunks[chunkIndex] = new(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.DeserializationChunkTag), chunkStart);

        outputStream.Seek(chunkLocationPosition, SeekOrigin.Begin);
        
        foreach ((uint chunkIdentifier, int chunkSize) in chunks) {
            bwriter.Write(chunkIdentifier);
            bwriter.Write(chunkSize);
        }
    }
    
    private static void WriteDataChunk(BinaryWriter writer, Serializer serializer) {
        Stream outputStream = writer.BaseStream;
        
        writer.Write(CompilingConstants.ResourceDataChunkTag);
        {
            var chunkLenPosition = (int)outputStream.Position;
            writer.Seek(4, SeekOrigin.Current);
            
            serializer.SerializeObject(outputStream);
            
            var serializedSize = (int)(outputStream.Position - chunkLenPosition - 4);
        
            writer.Seek(chunkLenPosition, SeekOrigin.Begin);
            writer.Write(serializedSize);
            
            writer.Seek(serializedSize, SeekOrigin.Current);
        }
    }
    
    private static void WriteOptionsChunk(BinaryWriter writer, Serializer serializer) {
        Stream outputStream = writer.BaseStream;
        
        writer.Write(CompilingConstants.ImportOptionsChunkTag);
        {
            var chunkLenPosition = (int)outputStream.Position;
            writer.Seek(4, SeekOrigin.Current);
            
            serializer.SerializeOptions(outputStream);
            
            var serializedSize = (int)(outputStream.Position - chunkLenPosition - 4);
        
            writer.Seek(chunkLenPosition, SeekOrigin.Begin);
            writer.Write(serializedSize);
            
            writer.Seek(serializedSize, SeekOrigin.Current);
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
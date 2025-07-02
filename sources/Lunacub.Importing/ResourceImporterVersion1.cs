using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Importing.Extensions;

namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceImporterVersion1 {
    public async Task<ResourceCache.VesselDeserializeResult> ImportVessel(
        ImportEnvironment environment,
        Type resourceType,
        Stream resourceStream,
        BinaryHeader header,
        CancellationToken cancellationToken
    ) {
        bool disposeResourceStream = true;

        try {
            if (header.MinorVersion != 0) {
                throw new NotSupportedException($"Compiled resource version 1.{header.MinorVersion} is not supported.");
            }

            if (!header.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out ChunkInformation dataChunk)) {
                string message = string.Format(
                    ExceptionMessages.ResourceMissingChunk,
                    Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)
                );
                throw new CorruptedBinaryException(message);
            }

            if (!header.TryGetChunkInformation(CompilingConstants.DeserializationChunkTag, out ChunkInformation deserializationChunk)) {
                string message = string.Format(
                    ExceptionMessages.ResourceMissingChunk,
                    Encoding.ASCII.GetString(CompilingConstants.DeserializationChunkTag)
                );
                throw new CorruptedBinaryException(message);
            }

            resourceStream.Seek(deserializationChunk.ContentOffset, SeekOrigin.Begin);

            using BinaryReader reader = new(resourceStream, Encoding.UTF8, true);

            string deserializerName = reader.ReadString();

            if (!environment.Deserializers.TryGetValue(deserializerName, out Deserializer? deserializer)) {
                string message = string.Format(ExceptionMessages.UnregisteredDeserializer, deserializerName);
                throw new ArgumentException(message);
            }

            if (!deserializer.OutputType.IsAssignableTo(resourceType)) {
                return default;
            }

            await using Stream optionsStream = await CopyOptionsStreamAsync(resourceStream, header, cancellationToken);

            resourceStream.Seek(dataChunk.ContentOffset, SeekOrigin.Begin);
            optionsStream.Seek(0, SeekOrigin.Begin);

            await using PartialReadStream dataStream = new(resourceStream, dataChunk.ContentOffset, dataChunk.Length, false);
            DeserializationContext context = new(environment.Logger);

            object deserialized = await deserializer.DeserializeObjectAsync(dataStream, optionsStream, context, cancellationToken);

            if (deserializer.Streaming) {
                disposeResourceStream = false;
            }

            return new(deserializer, deserialized, context);
        } finally {
            if (disposeResourceStream) await resourceStream.DisposeAsync();
        }
    }
    
    private static async Task<Stream> CopyOptionsStreamAsync(Stream stream, BinaryHeader header, CancellationToken token) {
        if (header.TryGetChunkInformation(CompilingConstants.ImportOptionsChunkTag, out ChunkInformation optionsChunkInfo)) {
            stream.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
            
            MemoryStream optionsStream = new((int)optionsChunkInfo.Length);
            await stream.CopyToAsync(optionsStream, (int)optionsChunkInfo.Length, token, 128);
    
            return optionsStream;
        }
        
        return Stream.Null;
    }
}
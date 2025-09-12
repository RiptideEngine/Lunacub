using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Helpers;
using Caxivitual.Lunacub.Importing.Extensions;
using Microsoft.IO;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static class ResourceImporterVersion1 {
    public static async Task<ResourceImportDispatcher.ResourceImportResult> ImportVessel(
        ImportEnvironment environment,
        Stream resourceStream,
        Header header,
        ReadOnlyMemory<ChunkPositionalInformation> chunkPositionalInfos,
        CancellationToken cancellationToken
    ) {
        if (header.MinorVersion != 0) {
            throw new NotSupportedException($"Compiled resource version 1.{header.MinorVersion} is not supported.");
        }
        
        if (!chunkPositionalInfos.TryGet(CompilingConstants.ResourceDataChunkTag, out ChunkPositionalInformation dataChunk)) {
            string message = string.Format(
                ExceptionMessages.ResourceMissingChunk,
                CompilingConstants.ResourceDataChunkTag.AsAsciiString
            );
            throw new CorruptedBinaryException(message);
        }

        if (!chunkPositionalInfos.TryGet(CompilingConstants.DeserializationChunkTag, out ChunkPositionalInformation deserializationChunk)) {
            string message = string.Format(
                ExceptionMessages.ResourceMissingChunk,
                CompilingConstants.DeserializationChunkTag.AsAsciiString
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

        await using Stream optionsStream = await CopyOptionsStreamAsync(environment.MemoryStreamManager, resourceStream, header, chunkPositionalInfos, cancellationToken);

        resourceStream.Seek(dataChunk.ContentOffset, SeekOrigin.Begin);
        optionsStream.Seek(0, SeekOrigin.Begin);

        await using PartialReadStream dataStream = new(resourceStream, dataChunk.ContentOffset, dataChunk.Length, false);
        DeserializationContext context = new(environment.Logger, environment.MemoryStreamManager);

        object deserialized = await deserializer.DeserializeObjectAsync(dataStream, optionsStream, context, cancellationToken);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (deserialized == null) {
            throw new InvalidOperationException("Deserializer cannot returns null object.");
        }

        if (!deserialized.GetType().IsAssignableTo(deserializer.OutputType)) {
            if (environment.Disposers.TryDispose(deserialized)) {
                environment.Statistics.IncrementDisposedResourceCount();
            } else {
                environment.Statistics.IncrementUndisposedResourceCount();
            }

            throw new InvalidOperationException("Deserialized object cannot be assigned to Deserializer's output type.");
        }

        return new(deserializer, deserialized, context);
    }
    
    private async static Task<Stream> CopyOptionsStreamAsync(RecyclableMemoryStreamManager memoryStreamManager, Stream stream, Header header, ReadOnlyMemory<ChunkPositionalInformation> chunkPositionalInfos, CancellationToken token) {
        if (chunkPositionalInfos.TryGet(CompilingConstants.ImportOptionsChunkTag, out ChunkPositionalInformation optionsChunkInfo)) {
            stream.Seek(optionsChunkInfo.ContentOffset, SeekOrigin.Begin);
            
            RecyclableMemoryStream optionsStream = memoryStreamManager.GetStream("OptionsStream", optionsChunkInfo.Length, false);
            await stream.CopyToAsync(optionsStream, (int)optionsChunkInfo.Length, token, 256);
    
            return optionsStream;
        }
        
        return Stream.Null;
    }
}
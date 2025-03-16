﻿using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceRegistry {
    private object? ImportInner(ResourceID rid, Type type) {
        Debug.Assert(_lock.IsWriteLockHeld);

        Dictionary<ResourceID, object> imported = [];

        return ImportRecursive(rid, type, imported);
    }

    private object? ImportRecursive(ResourceID rid, Type type, Dictionary<ResourceID, object> importedStack) {
        if (_resources.TryGetValue(rid, out var cache)) return cache;
        if (importedStack.TryGetValue(rid, out var imported)) return imported;
        
        using Stream resourceStream = _context.Input.CreateResourceStream(rid);
        ExtractCompiledResource(resourceStream, out ushort major, out ushort minor, out ChunkLookupTable table);
                
        return major switch {
            1 => ImportV1(minor, rid, resourceStream, type, in table, importedStack),
            _ => throw new NotSupportedException($"Compiled resource version {major} is not supported."),
        };
    }

    private object? ImportV1(ushort minor, ResourceID rid, Stream resourceStream, Type type, in ChunkLookupTable table, Dictionary<ResourceID, object> importedStack) {
        if (minor != 0) {
            throw new NotSupportedException($"Compiled resource version 1.{minor} is not supported.");
        }

        if (!table.TryGetChunkPosition(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.DeserializationChunkTag), out int deserializationChunkPosition)) {
            throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
        }

        using BinaryReader reader = new(resourceStream, Encoding.UTF8, true);
        
        resourceStream.Seek(deserializationChunkPosition + 8, SeekOrigin.Begin);

        string deserializerName = reader.ReadString();

        if (!_context.Deserializers.TryGetValue(deserializerName, out Deserializer? deserializer)) {
            throw new ArgumentException($"Deserializer name '{deserializerName}' is unregistered.");
        }

        if (!deserializer.OutputType.IsAssignableTo(type)) {
            return null;
        }
        
        if (!table.TryGetChunkPosition(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ResourceDataChunkTag), out int dataChunkPosition)) {
            throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
        }
        
        // TODO: Pass in a partial Stream object instead the whole resource stream.
        resourceStream.Seek(dataChunkPosition + 4, SeekOrigin.Begin);

        // TODO: Context, reference handling.
        DeserializationContext context = new();
        object deserialized = deserializer.DeserializeObject(resourceStream, context);
        
        importedStack.Add(rid, deserialized);
        
        Dictionary<string, object?> importedDependencies = [];

        foreach ((string property, DeserializationContext.RequestingDependency requesting) in context.RequestingDependencies) {
            importedDependencies.Add(property, ImportRecursive(requesting.Rid, requesting.Type, importedStack));
        }

        context.Dependencies = importedDependencies;
        deserializer.ResolveDependencies(deserialized, context);
        
        _resources.Add(rid, deserialized);
        
        return deserialized;
    }
    
    private static void ExtractCompiledResource(Stream stream, out ushort majorVersion, out ushort minorVersion, out ChunkLookupTable lookupTable) {
        int read;
        using BinaryReader br = new(stream, Encoding.UTF8, true);
        
        unsafe {
            uint magic;
            read = br.Read(new Span<byte>(&magic, sizeof(uint)));

            if (read < 4) throw new ArgumentException("Failed to read magic number.");

            if (magic != BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.MagicIdentifier)) {
                throw new FormatException("Invalid magic number.");
            }
        }

        majorVersion = br.ReadUInt16();
        minorVersion = br.ReadUInt16();

        int numChunks = br.ReadInt32();

        lookupTable = new(8);

        for (int i = 0; i < numChunks; i++) {
            lookupTable.Add(new(br.ReadUInt32(), br.ReadInt32()));
        }

        ValidateLookupTable(br, in lookupTable);
    }

    private static void ValidateLookupTable(BinaryReader reader, in ChunkLookupTable lookupTable) {
        Stream baseStream = reader.BaseStream;
        
        foreach ((uint chunkTag, int position) in lookupTable.Span) {
            ReadOnlySpan<byte> chunkTagBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in chunkTag, 1));
            
            if (position >= baseStream.Length) throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has position surpassed Stream's length.");

            baseStream.Seek(position, SeekOrigin.Begin);

            uint validatingChunk;
            
            try {
                validatingChunk = reader.ReadUInt32();
            } catch (EndOfStreamException e) {
                throw new CorruptedFormatException($"Failed to validate chunk {Encoding.ASCII.GetString(chunkTagBytes)} (Unreadable tag).", e);
            }

            if (validatingChunk != chunkTag) {
                throw new CorruptedFormatException($"Expected chunk tag {Encoding.ASCII.GetString(chunkTagBytes)} at position {position}.");
            }
            
            uint chunkLength;
            
            try {
                chunkLength = reader.ReadUInt32();
            } catch (EndOfStreamException e) {
                throw new CorruptedFormatException($"Failed to validate chunk {Encoding.ASCII.GetString(chunkTagBytes)} (Unreadable length).", e);
            }

            if (baseStream.Position + chunkLength > baseStream.Length) {
                throw new CorruptedFormatException($"Chunk {Encoding.ASCII.GetString(chunkTagBytes)} has length surpassed Stream's length.");
            }
        }
    }
}
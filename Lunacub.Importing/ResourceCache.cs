using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceCache(ImportingContext context) : IDisposable {
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    private readonly Dictionary<ResourceID, object> _resources = new();
    
    private bool _disposed;

    public object Import(ResourceID rid, string path, Type resourceType) {
        _lock.EnterUpgradeableReadLock();
        try {
            if (_resources.TryGetValue(rid, out var imported)) {
                Debug.Assert(imported.GetType().IsAssignableTo(resourceType));

                return imported;
            }

            string resourceFullPath = Path.GetFullPath(path);

            if (!File.Exists(resourceFullPath)) {
                throw new FileNotFoundException($"Resource at path '{resourceFullPath}' does not exist.");
            }

            _lock.EnterWriteLock();
            try {
                using FileStream stream = new(resourceFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                ExtractCompiledResource(stream, out ushort major, out ushort minor, out ChunkLookupTable chunkTable);
                
                return major switch {
                    1 => ImportV1(minor, stream, in chunkTable),
                    _ => throw new NotSupportedException($"Compiled resource version {major} is not supported.")
                };
            } finally {
                _lock.ExitWriteLock();
            }
        } finally {
            _lock.ExitUpgradeableReadLock();
        }

        object ImportV1(ushort minor, Stream stream, in ChunkLookupTable chunkTable) {
            if (minor != 0) {
                throw new NotSupportedException($"Compiled resource version 1.{minor} is not supported.");
            }

            if (!chunkTable.TryGetChunkPosition(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.ResourceDataChunkTag), out int dataChunkPosition)) {
                throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.ResourceDataChunkTag)} chunk.");
            }
            
            if (!chunkTable.TryGetChunkPosition(BinaryPrimitives.ReadUInt32LittleEndian(CompilingConstants.DeserializationChunkTag), out int deserializationChunkPosition)) {
                throw new CorruptedFormatException($"Compiled resource missing {Encoding.ASCII.GetString(CompilingConstants.DeserializationChunkTag)} chunk.");
            }

            using BinaryReader reader = new(stream, Encoding.UTF8, true);

            stream.Seek(deserializationChunkPosition, SeekOrigin.Begin);

            string deserializerName = reader.ReadString();

            if (!context.Deserializers.TryGetValue(deserializerName, out var deserializer)) {
                throw new ArgumentException($"Deserializer '{deserializerName}' not found.");
            }

            return deserializer.DeserializeObject(stream, new());
        }
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

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            _lock.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceCache() {
        Dispose(false);
    }

    private readonly record struct DeserializationChunk(string DeserializerName);
    private readonly record struct ResourceDataChunk(int Length);
}
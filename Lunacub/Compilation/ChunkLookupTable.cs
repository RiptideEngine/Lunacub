using System.Buffers;

namespace Caxivitual.Lunacub.Compilation;

public struct ChunkLookupTable : IDisposable {
    public ChunkTagPosition[] _array;

    public int Count { get; private set; }
    public int Capacity => _array.Length;

    public readonly ReadOnlySpan<ChunkTagPosition> Span => _array.AsSpan(0,Count);

    public ChunkLookupTable() {
        _array = [];
    }

    public ChunkLookupTable(int initialCount) {
        _array = ArrayPool<ChunkTagPosition>.Shared.Rent(initialCount);
    }

    public void Add(ChunkTagPosition item) {
        if (Count == Capacity) {
            ChunkTagPosition[] rent = ArrayPool<ChunkTagPosition>.Shared.Rent(Count + 16);
            Span.CopyTo(rent);
            ArrayPool<ChunkTagPosition>.Shared.Return(_array);
            _array = rent;
        }

        _array[Count++] = item;
    }
    
    public void Add(uint tag, int position) => Add(new(tag, position));

    public readonly bool TryGetChunkPosition(uint tag, out int position) {
        foreach (ref readonly var entry in Span) {
            if (entry.Tag == tag) {
                position = entry.Position;
                return true;
            }
        }

        position = 0;
        return false;
    }

    public void Dispose() {
        if (_array != null) {
            ArrayPool<ChunkTagPosition>.Shared.Return(_array);
            _array = [];
        }
    }
}
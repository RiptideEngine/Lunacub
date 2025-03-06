using System.Buffers;

namespace Caxivitual.Lunacub.Compilation;

public ref struct ChunkLookupTable : IDisposable {
    private ChunkTagPosition[] _array;
    private Span<ChunkTagPosition> _span;
    private int _position;

    public int Count => _position;
    public int Capacity => _span.Length;

    public readonly ReadOnlySpan<ChunkTagPosition> Span => _span[.._position];
    
    public ChunkLookupTable(int initialCount) {
        _array = ArrayPool<ChunkTagPosition>.Shared.Rent(initialCount);
        _span = _array;
    }

    public void Add(ChunkTagPosition item) {
        if (Count == Capacity) {
            ChunkTagPosition[] rent = ArrayPool<ChunkTagPosition>.Shared.Rent(_position + 16);
            Span.CopyTo(rent);
            ArrayPool<ChunkTagPosition>.Shared.Return(_array);
            _array = rent;
            _span = _array;
        }

        _span[_position++] = item;
    }

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
        ArrayPool<ChunkTagPosition>.Shared.Return(_array);
        _array = [];
    }
}
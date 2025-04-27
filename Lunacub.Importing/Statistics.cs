namespace Caxivitual.Lunacub.Importing;

public sealed class Statistics {
    private uint _totalReferenceCount, _totalDisposeCount, _uniqueResourceCount, _disposedResourceCount, _undisposedResourceCount;

    public uint TotalReferenceCount => _totalReferenceCount;
    public uint TotalDisposeCount => _totalDisposeCount;
    public uint UniqueResourceCount => _uniqueResourceCount;
    public uint DisposedResourceCount => _disposedResourceCount;
    public uint UndisposedResourceCount => _undisposedResourceCount;
    
    internal Statistics() { }

    internal void IncrementTotalReferenceCount() {
        Interlocked.Increment(ref _totalReferenceCount);
    }

    internal void DecrementTotalReferenceCount() {
        uint initialValue, computedValue;
        do {
            initialValue = _totalReferenceCount;
            computedValue = initialValue == 0 ? 0 : initialValue - 1;
        } while (initialValue != Interlocked.CompareExchange(ref _totalReferenceCount, computedValue, initialValue));
    }
    
    internal void IncrementTotalDisposeCount() {
        Interlocked.Increment(ref _totalDisposeCount);
    }

    internal void DecrementTotalDisposeCount() {
        uint initialValue, computedValue;
        do {
            initialValue = _totalDisposeCount;
            computedValue = initialValue == 0 ? 0 : initialValue - 1;
        } while (initialValue != Interlocked.CompareExchange(ref _totalDisposeCount, computedValue, initialValue));
    }
    
    internal void IncrementUniqueResourceCount() {
        Interlocked.Increment(ref _uniqueResourceCount);
    }

    internal void DecrementUniqueResourceCount() {
        uint initialValue, computedValue;
        do {
            initialValue = _uniqueResourceCount;
            computedValue = initialValue == 0 ? 0 : initialValue - 1;
        } while (initialValue != Interlocked.CompareExchange(ref _uniqueResourceCount, computedValue, initialValue));
    }
    
    internal void IncrementDisposedResourceCount() {
        Interlocked.Increment(ref _disposedResourceCount);
    }

    internal void DecrementDisposedResourceCount() {
        uint initialValue, computedValue;
        do {
            initialValue = _disposedResourceCount;
            computedValue = initialValue == 0 ? 0 : initialValue - 1;
        } while (initialValue != Interlocked.CompareExchange(ref _disposedResourceCount, computedValue, initialValue));
    }
    
    internal void IncrementUndisposedResourceCount() {
        Interlocked.Increment(ref _undisposedResourceCount);
    }

    internal void DecrementUndisposedResourceCount() {
        uint initialValue, computedValue;
        do {
            initialValue = _undisposedResourceCount;
            computedValue = initialValue == 0 ? 0 : initialValue - 1;
        } while (initialValue != Interlocked.CompareExchange(ref _undisposedResourceCount, computedValue, initialValue));
    }
    
    internal void SetTotalReferenceCount(uint value) => _totalReferenceCount = value;
    internal void SetTotalDisposeCount(uint value) => _totalDisposeCount = value;
    internal void SetUniqueResourceCount(uint value) => _uniqueResourceCount = value;
    internal void SetDisposedResourceCount(uint value) => _disposedResourceCount = value;
    internal void SetUndisposedResourceCount(uint value) => _undisposedResourceCount = value;
}
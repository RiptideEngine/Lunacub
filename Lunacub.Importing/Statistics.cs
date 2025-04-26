namespace Caxivitual.Lunacub.Importing;

public sealed class Statistics {
    private uint _totalReferenceCount, _totalDisposeCount, _uniqueResourceCount;

    public uint TotalReferenceCount => _totalReferenceCount;
    public uint TotalDisposeCount => _totalDisposeCount;

    public uint UniqueResourceCount => _uniqueResourceCount;
    
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
    
    internal void SetTotalReferenceCount(uint value) => _totalReferenceCount = value;
    internal void SetTotalDisposeCount(uint value) => _totalDisposeCount = value;
    internal void SetUniqueResourceCount(uint value) => _uniqueResourceCount = value;
}
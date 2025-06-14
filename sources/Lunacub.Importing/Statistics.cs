namespace Caxivitual.Lunacub.Importing;

public sealed class Statistics {
    private AtomicCounter _totalReferenceCount, _remainReferenceCount;
    private AtomicCounter _uniqueResourceCount, _disposedResourceCount, _undisposedResourceCount;
    
    public uint TotalReferenceCount => _totalReferenceCount;
    public uint RemainReferenceCount => _remainReferenceCount;
    public uint UniqueResourceCount => _uniqueResourceCount;
    public uint DisposedResourceCount => _disposedResourceCount;
    public uint UndisposedResourceCount => _undisposedResourceCount;
    
    internal Statistics() { }

    internal void AddReference() {
        _totalReferenceCount.Increment();
        _remainReferenceCount.Increment();
    }

    internal void Release(uint count = 1) {
        _remainReferenceCount.Subtract(count);
    }
    
    internal void IncrementUniqueResourceCount() {
        _uniqueResourceCount.Increment();
    }
    
    internal void DecrementUniqueResourceCount() {
        _uniqueResourceCount.Decrement();
    }
    
    internal void IncrementDisposedResourceCount() {
        _disposedResourceCount.Increment();
    }
    
    internal void DecrementDisposedResourceCount() {
        _disposedResourceCount.Decrement();
    }
    
    internal void IncrementUndisposedResourceCount() {
        _undisposedResourceCount.Increment();
    }
    
    internal void DecrementUndisposedResourceCount() {
        _undisposedResourceCount.Decrement();
    }

    internal void SetTotalReferenceCount(uint value) => _totalReferenceCount = value;
    internal void SetRemainReferenceCount(uint value) => _remainReferenceCount = value;
    internal void SetUniqueResourceCount(uint value) => _uniqueResourceCount = value;
    internal void SetDisposedResourceCount(uint value) => _disposedResourceCount = value;
    internal void SetUndisposedResourceCount(uint value) => _undisposedResourceCount = value;

    private struct AtomicCounter {
        private uint _value;
        public uint Value => _value;

        private AtomicCounter(uint value) {
            _value = value;
        }
        
        public void Increment() {
            Interlocked.Increment(ref _value);
        }

        public void Decrement() {
            uint initialValue, computedValue;
            do {
                initialValue = _value;
                computedValue = _value == 0 ? 0 : _value - 1;
            } while (initialValue != Interlocked.CompareExchange(ref _value, computedValue, initialValue));
        }

        public void Subtract(uint count) {
            uint initialValue, computedValue;
            do {
                initialValue = _value;
                computedValue = count >= _value ? 0 : _value - count;
            } while (initialValue != Interlocked.CompareExchange(ref _value, computedValue, initialValue));
        }
        
        public static implicit operator uint(AtomicCounter counter) => counter._value;
        public static implicit operator AtomicCounter(uint value) => new(value);
    }
}
namespace Caxivitual.Lunacub.Importing;

internal struct DetachableDisposable<T>(T value) : IDisposable where T : IDisposable {
    public T? Value { get; private set; } = value;

    public T? Detach() {
        T? output = Value;
        Value = default!;
        return output;
    }
    
    public void Dispose() {
        if (Value != null) {
            Value.Dispose();
        }
    }
}
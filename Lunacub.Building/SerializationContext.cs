namespace Caxivitual.Lunacub.Building;

public sealed class SerializationContext {
    public object? Options { get; }

    internal SerializationContext(object? options) {
        Options = options;
    }
}
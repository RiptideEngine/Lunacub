namespace Caxivitual.Lunacub.Building;

public sealed class ProcessingContext {
    public object? Options { get; }

    internal ProcessingContext(object? options) {
        Options = options;
    }
}
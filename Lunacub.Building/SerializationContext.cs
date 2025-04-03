namespace Caxivitual.Lunacub.Building;

public sealed class SerializationContext {
    public IImportOptions? Options { get; }

    internal SerializationContext(IImportOptions? options) {
        Options = options;
    }
}
namespace Caxivitual.Lunacub.Building;

public sealed class ProcessingContext {
    public IImportOptions? Options { get; }

    internal ProcessingContext(IImportOptions? options) {
        Options = options;
    }
}
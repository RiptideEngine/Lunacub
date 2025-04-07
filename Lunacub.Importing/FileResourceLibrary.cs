using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
public class FileResourceLibrary(Guid id, string directory) : ResourceLibrary(id) {
    public string Directory { get; } = directory;
    
    public override bool Contains(ResourceID rid) {
        return File.Exists(Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
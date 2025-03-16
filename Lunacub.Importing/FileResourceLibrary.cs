using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Importing;

public class FileResourceLibrary(Guid id, string directory) : ResourceLibrary(id) {
    public string Directory { get; } = directory;
    
    public override bool Contains(ResourceID rid) {
        return File.Exists(Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    public override Stream CreateStream(ResourceID rid) {
        return new FileStream(Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}"), FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
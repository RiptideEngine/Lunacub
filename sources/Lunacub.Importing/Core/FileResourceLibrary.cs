using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileResourceLibrary : ImportResourceLibrary {
    public string RootDirectory { get; }

    public FileResourceLibrary(string rootDirectory) {
        if (!Directory.Exists(rootDirectory)) {
            throw new DirectoryNotFoundException($"Resource directory '{rootDirectory}' not found.");
        }
        
        RootDirectory = rootDirectory;
    }
    
    protected override Stream? CreateResourceStreamCore(ResourceID rid, PrimitiveRegistryElement element) {
        string path = Path.Combine(RootDirectory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
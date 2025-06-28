using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileResourceLibrary : ImportResourceLibrary {
    public string RootDirectory { get; }

    public FileResourceLibrary(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException($"Resource directory '{RootDirectory}' not found.");
        }
    }

    protected override Stream? CreateResourceStreamCore(ResourceID resourceId, byte options) {
        string path = Path.Combine(RootDirectory, $"{resourceId:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
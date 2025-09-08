using Caxivitual.Lunacub.Compilation;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileSourceRepository : SourceRepository {
    public string RootDirectory { get; }

    public FileSourceRepository(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException($"Resource library directory '{RootDirectory}' not found.");
        }
    }

    protected override Stream? CreateStreamCore(ResourceID id) {
        string path = Path.Combine(RootDirectory, $"{id:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
using Caxivitual.Lunacub.Compilation;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileSourceProvider : ImportSourceProvider {
    public string RootDirectory { get; }

    public FileSourceProvider(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException($"Resource directory '{RootDirectory}' not found.");
        }
    }

    protected override Stream? CreateStreamCore(ResourceAddress resourceAddress) {
        string path = Path.Combine(
            RootDirectory,
            resourceAddress.LibraryId.ToString(),
            $"{resourceAddress.ResourceId}{CompilingConstants.CompiledResourceExtension}"
        );
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
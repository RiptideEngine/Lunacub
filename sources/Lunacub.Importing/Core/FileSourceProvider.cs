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

    protected override Stream? CreateStreamCore(ResourceID resourceId) {
        string path = Path.Combine(RootDirectory, $"{resourceId:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    protected override Stream? CreateStreamCore(string address) {
        if (ResourceID.TryParse(Path.GetFileNameWithoutExtension(address), NumberStyles.HexNumber, null, out var resourceId)) {
            return CreateStreamCore(resourceId);
        }

        return null;
    }
}
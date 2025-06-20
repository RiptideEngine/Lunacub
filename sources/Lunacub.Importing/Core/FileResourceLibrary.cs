using Caxivitual.Lunacub.Compilation;
using System.Globalization;
using System.Text.Json;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileResourceLibrary : ResourceLibrary {
    public string Directory { get; }
    
    public override ResourceRegistry Registry { get; }

    public FileResourceLibrary(string directory) {
        Directory = directory;

        string registryFilePath = Path.Combine(directory, "__registry");

        using (FileStream fs = new FileStream(registryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            Registry = JsonSerializer.Deserialize<ResourceRegistry>(fs) ?? [];
        }
    }
    
    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = Path.Combine(Directory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
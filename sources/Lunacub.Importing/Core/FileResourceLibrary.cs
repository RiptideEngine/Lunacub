using Caxivitual.Lunacub.Compilation;
using System.Globalization;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileResourceLibrary(string directory) : ResourceLibrary {
    public string Directory { get; } = directory;
    
    public override bool Contains(ResourceID rid) {
        return File.Exists(Path.Combine(Directory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}"));
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = Path.Combine(Directory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    public override IEnumerator<ResourceID> GetEnumerator() {
        foreach (var file in System.IO.Directory.EnumerateFiles(Directory, $"*{CompilingConstants.CompiledResourceExtension}")) {
            if (ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, null, out var rid)) {
                yield return rid;
            }
        }
    }
}
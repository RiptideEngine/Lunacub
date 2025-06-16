using System.Globalization;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class MockResourceLibrary(MockFileSystem fs) : ResourceLibrary {
    public MockFileSystem FileSystem { get; } = fs;

    public override bool Contains(ResourceID rid) {
        return FileSystem.File.Exists(FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }

    public override IEnumerator<ResourceID> GetEnumerator() {
        foreach (var file in FileSystem.Directory.EnumerateFiles(MockOutputSystem.ResourceOutputDirectory, $"*{CompilingConstants.CompiledResourceExtension}")) {
            if (ResourceID.TryParse(FileSystem.Path.GetFileNameWithoutExtension(file.AsSpan()), NumberStyles.HexNumber, null, out var rid)) {
                yield return rid;
            }
        }
    }
}
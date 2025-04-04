namespace Caxivitual.Lunacub.Tests.Common;

public class MockResourceLibrary(Guid id, MockFileSystem fs) : ResourceLibrary(id) {
    public MockFileSystem FileSystem { get; } = fs;

    public override bool Contains(ResourceID rid) {
        return FileSystem.File.Exists(FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }
}
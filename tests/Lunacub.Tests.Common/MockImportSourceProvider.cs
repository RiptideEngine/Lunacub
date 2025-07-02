namespace Caxivitual.Lunacub.Tests.Common;

public sealed class MockImportSourceProvider : ImportSourceProvider {
    public MockFileSystem FileSystem { get; }

    public MockImportSourceProvider(MockFileSystem fs) {
        FileSystem = fs;
    }

    protected override Stream? CreateStreamCore(ResourceID resourceId) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{resourceId}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }

    protected override Stream? CreateStreamCore(string address) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, address);
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }
}
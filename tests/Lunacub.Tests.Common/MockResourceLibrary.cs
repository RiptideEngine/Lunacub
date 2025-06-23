namespace Caxivitual.Lunacub.Tests.Common;

public sealed class MockResourceLibrary : ResourceLibrary {
    public MockFileSystem FileSystem { get; }

    public override ResourceRegistry Registry { get; }

    public MockResourceLibrary(MockFileSystem fs) {
        FileSystem = fs;
        
        string registryFilePath = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, "__registry");

        using (Stream stream = FileSystem.File.OpenRead(registryFilePath)) {
            Registry = JsonSerializer.Deserialize<ResourceRegistry>(stream) ?? [];
        }
    }
    
    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }
}
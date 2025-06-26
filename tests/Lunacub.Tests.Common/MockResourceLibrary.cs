namespace Caxivitual.Lunacub.Tests.Common;

public sealed class MockResourceLibrary : ImportResourceLibrary {
    public MockFileSystem FileSystem { get; }

    public MockResourceLibrary(MockFileSystem fs) {
        FileSystem = fs;
        
        string registryFilePath = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, "__registry");

        using (Stream stream = FileSystem.File.OpenRead(registryFilePath)) {
            foreach ((var k, var v) in JsonSerializer.Deserialize<ResourceRegistry<PrimitiveRegistryElement>>(stream) ?? []) {
                Registry.Add(k, v);
            }
        }
    }
    
    protected override Stream? CreateResourceStreamCore(ResourceID rid, PrimitiveRegistryElement element) {
        string path = FileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.OpenRead(path) : null;
    }
}
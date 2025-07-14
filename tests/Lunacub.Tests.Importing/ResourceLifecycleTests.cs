namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceLifecycleTests {
    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnvironment;
    private readonly ImportEnvironment _importEnvironment;
    
    public ResourceLifecycleTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _fileSystem = new();
        
        _buildEnvironment = new(new MockOutputSystem(_fileSystem));
        _importEnvironment = new() {
            Logger = output.BuildLogger(),
        };
        
        componentsFixture.ApplyComponents(_buildEnvironment);
        componentsFixture.ApplyComponents(_importEnvironment);
    }
}
using System.Reflection;

namespace Caxivitual.Lunacub.Tests.Importing;

public partial class ImportEnvironmentTests : IClassFixture<ImportingTestFixture>, IDisposable {
    private readonly ImportingTestFixture _fixture;

    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnv;
    private readonly ImportEnvironment _importEnv;
    private readonly ITestOutputHelper _output;
    
    public ImportEnvironmentTests(ImportingTestFixture fixture, ITestOutputHelper output) {
        _fixture = fixture;
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _fileSystem = new();
        
        _buildEnv = new(new MockOutputSystem(_fileSystem));
        _importEnv = new();

        foreach (var type in _fixture.ComponentTypes[typeof(Importer)]) {
            _buildEnv.Importers.Add(type.Name, (Importer)Activator.CreateInstance(type)!);
        }
        
        foreach (var type in _fixture.ComponentTypes[typeof(SerializerFactory)]) {
            _buildEnv.SerializerFactories.Add((SerializerFactory)Activator.CreateInstance(type)!);
        }
        
        foreach (var type in _fixture.ComponentTypes[typeof(Deserializer)]) {
            _importEnv.Deserializers.Add(type.Name, (Deserializer)Activator.CreateInstance(type)!);
        }
    }
    
    public void Dispose() {
        _importEnv.Dispose();
        _buildEnv.Dispose();
        
        GC.SuppressFinalize(this);
    }

    private void BuildResources(params ResourceID[] rids) {
        foreach (var rid in rids) {
            _fixture.GetResourceConfiguration(_buildEnv, rid);
        }
        
        _buildEnv.BuildResources();
    }
}
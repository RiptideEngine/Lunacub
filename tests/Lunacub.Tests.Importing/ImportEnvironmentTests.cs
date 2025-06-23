namespace Caxivitual.Lunacub.Tests.Importing;

public partial class ImportEnvironmentTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ComponentsFixture _componentsFixture;

    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnvironment;
    private readonly ImportEnvironment _importEnvironment;
    private readonly ITestOutputHelper _output;
    
    public ImportEnvironmentTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _componentsFixture = componentsFixture;
        _output = output;
        DebugHelpers.RedirectConsoleOutput(output);

        _fileSystem = new();
        
        _buildEnvironment = new(new MockOutputSystem(_fileSystem));
        _importEnvironment = new();

        foreach (var type in _componentsFixture.ComponentTypes[typeof(Importer)]) {
            _buildEnvironment.Importers.Add(type.Name, (Importer)Activator.CreateInstance(type)!);
        }
        
        foreach (var type in _componentsFixture.ComponentTypes[typeof(Processor)]) {
            _buildEnvironment.Processors.Add(type.Name, (Processor)Activator.CreateInstance(type)!);
        }
        
        foreach (var type in _componentsFixture.ComponentTypes[typeof(SerializerFactory)]) {
            _buildEnvironment.SerializerFactories.Add((SerializerFactory)Activator.CreateInstance(type)!);
        }
        
        foreach (var type in _componentsFixture.ComponentTypes[typeof(Deserializer)]) {
            _importEnvironment.Deserializers.Add(type.Name, (Deserializer)Activator.CreateInstance(type)!);
        }
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        _buildEnvironment.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
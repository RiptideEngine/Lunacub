namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IClassFixture<BuildTestsFixture>, IDisposable {
    private readonly BuildEnvironment _env;
    private readonly ITestOutputHelper _output;
    private readonly BuildTestsFixture _fixture;

    public BuildEnvironmentTests(BuildTestsFixture fixture, ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _env = new(new MockOutputSystem());
        
        _env.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _env.SerializerFactories.Add(new SimpleResourceSerializerFactory());
        
        _env.Importers.Add(nameof(ReferenceResourceImporter), new ReferenceResourceImporter());
        _env.SerializerFactories.Add(new ReferenceResourceSerializerFactory());
        
        _env.Importers.Add(nameof(OptionsResourceImporter), new OptionsResourceImporter());
        _env.Processors.Add(nameof(OptionsResourceProcessor), new OptionsResourceProcessor());
        _env.SerializerFactories.Add(new OptionsResourceSerializerFactory());

        _fixture = fixture;
    }

    public void Dispose() {
        _env.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
}
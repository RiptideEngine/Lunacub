using Caxivitual.Lunacub.Building.Core;

namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IClassFixture<ResourcesFixture>, IDisposable {
    private readonly BuildEnvironment _env;
    private readonly ITestOutputHelper _output;
    private readonly ResourcesFixture _resourcesFixture;

    public BuildEnvironmentTests(ResourcesFixture resourcesFixture, ITestOutputHelper output) {
        _output = output;
        DebugHelpers.RedirectConsoleOutput(output);

        _env = new(new MockOutputSystem());
        
        _env.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _env.SerializerFactories.Add(new SimpleResourceSerializerFactory());
        
        _env.Importers.Add(nameof(ReferenceResourceImporter), new ReferenceResourceImporter());
        _env.SerializerFactories.Add(new ReferenceResourceSerializerFactory());
        
        _env.Importers.Add(nameof(OptionsResourceImporter), new OptionsResourceImporter());
        _env.Processors.Add(nameof(OptionsResourceProcessor), new OptionsResourceProcessor());
        _env.SerializerFactories.Add(new OptionsResourceSerializerFactory());

        _resourcesFixture = resourcesFixture;
    }

    public void Dispose() {
        _env.Dispose();
        GC.SuppressFinalize(this);
    }
}
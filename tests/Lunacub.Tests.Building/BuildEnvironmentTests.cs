namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly BuildEnvironment _environment;
    private readonly ComponentsFixture _componentsFixture;

    public BuildEnvironmentTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _componentsFixture = componentsFixture;

        _environment = new(new MockOutputSystem()) {
            Logger = output.BuildLogger(),
        };
        
        _componentsFixture.ApplyComponents(_environment);
    }

    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }
}
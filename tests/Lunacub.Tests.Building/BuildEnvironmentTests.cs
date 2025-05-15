namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ITestOutputHelper _output;
    private readonly BuildEnvironment _environment;
    private readonly ComponentsFixture _componentsFixture;

    public BuildEnvironmentTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _output = output;
        DebugHelpers.RedirectConsoleOutput(output);
        
        _componentsFixture = componentsFixture;

        _environment = new(new MockOutputSystem());
        
        _componentsFixture.ApplyComponents(_environment);
    }

    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }
}
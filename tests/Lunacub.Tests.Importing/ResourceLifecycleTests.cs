namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceLifecycleTests : IDisposable, IClassFixture<PrebuildResourcesFixture> {
    private readonly ImportEnvironment _importEnvironment;
    
    public ResourceLifecycleTests(PrebuildResourcesFixture fixture, ITestOutputHelper output) {
        _importEnvironment = fixture.CreateImportEnvironment();
        _importEnvironment.Logger = output.BuildLogger();
    }

    public void Dispose() {
        _importEnvironment.Dispose();
        GC.SuppressFinalize(this);
    }
}
namespace Caxivitual.Lunacub.Tests.Importing;

[Collection<PrebuildResourcesCollectionFixture>]
public class ImportEnvironmentTests : IDisposable {
    private readonly ImportEnvironment _importEnvironment;

    public ImportEnvironmentTests(PrebuildResourcesFixture fixture) {
        _importEnvironment = fixture.CreateImportEnvironment();
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        GC.SuppressFinalize(this);
    }
}
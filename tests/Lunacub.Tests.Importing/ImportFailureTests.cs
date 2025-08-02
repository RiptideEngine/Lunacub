namespace Caxivitual.Lunacub.Tests.Importing;

[Collection<PrebuildResourcesCollectionFixture>]
public class ImportFailureTests : IDisposable {
    private readonly ImportEnvironment _importEnvironment;
    
    public ImportFailureTests(PrebuildResourcesFixture fixture, ITestOutputHelper output) {
        _importEnvironment = fixture.CreateImportEnvironment();
        _importEnvironment.Logger = output.BuildLogger();
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    [Fact]
    public async Task FailureImport_UnregisteredResource_ReturnsCorrectStates() {
        _importEnvironment.Libraries.Add(new(new Lunacub.Importing.Core.MemorySourceProvider()));

        var operation = _importEnvironment.Import(PrebuildResourcesFixture.UnregisteredResourceID);
        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<ArgumentException>().WithMessage("*unregistered*");

        operation.Status.Should().Be(ImportingStatus.Failed);
        operation.UnderlyingContainer.CancellationTokenSource.Should().BeNull();
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task FailureImport_NullResourceStream_ReturnsCorrectStates() {
        _importEnvironment.Libraries.Add(new(new NullStreamSourceProvider()) {
            Registry = {
                [UInt128.MaxValue - 1] = new("Resource", []),
            },
        });

        var operation = _importEnvironment.Import(UInt128.MaxValue - 1);
        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<InvalidOperationException>().WithMessage("*null*stream*");
        
        operation.Status.Should().Be(ImportingStatus.Failed);
        operation.UnderlyingContainer.CancellationTokenSource.Should().BeNull();
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public void FailureDeserialization_ExceptionThrown_ReturnsCorrectState() {
        // TODO
    }
}
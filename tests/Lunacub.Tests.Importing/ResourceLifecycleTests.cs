using Microsoft.Extensions.Logging;

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
    
    [Fact]
    public async Task Import_SingleTime_ShouldHaveReferenceCountOf1() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();

        operation.Status.Should().Be(ImportingStatus.Success);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }
    
    [Fact]
    public async Task Import_MultipleTime_ReturnsSameResource() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        ResourceHandle h1 = (await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync()).Which;
        
        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        ResourceHandle h2 = (await new Func<Task<ResourceHandle>>(async () => await operation2).Should().NotThrowAsync()).Which;

        h1.Value.Should().BeSameAs(h2.Value);
    }
    
    [Fact]
    public async Task ReleaseByID_AfterImporting_ShouldBeDisposed() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();

        _importEnvironment.Release(operation.ResourceId).Should().BeOneOf(ReleaseStatus.Disposed, ReleaseStatus.NotDisposed);
        
        operation.Status.Should().Be(ImportingStatus.Disposed);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task ReleaseByID_DuringImporting_ShouldBeDisposed() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        _importEnvironment.Release(operation.ResourceId).Should().BeOneOf(ReleaseStatus.Canceled);
        
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
        
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void ReleaseByID_NotImportID_ReturnsNotimportedStatus() {
        new Func<ReleaseStatus>(() => _importEnvironment.Release(UInt128.MaxValue)).Should().NotThrow().Which.Should().Be(ReleaseStatus.NotImported);
    }

    [Fact]
    public void ReleaseByID_NullID_ReturnsNullStatus() {
        new Func<ReleaseStatus>(() => _importEnvironment.Release(ResourceID.Null)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Null);
    }
    
    [Fact]
    public async Task ReleaseByResource_AfterImport_ShouldDisposeCorrectly() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        ResourceHandle handle = (await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync()).Which;

        _importEnvironment.Release(handle.Value).Should().BeOneOf(ReleaseStatus.Disposed, ReleaseStatus.NotDisposed);
        
        operation.Status.Should().Be(ImportingStatus.Disposed);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task ReleaseByOperation_AfterImport_ShouldDisposeCorrectly() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();

        _importEnvironment.Release(operation).Should().BeOneOf(ReleaseStatus.Disposed, ReleaseStatus.NotDisposed);
        
        operation.Status.Should().Be(ImportingStatus.Disposed);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task ReleaseByHandle_AfterImport_ShouldDisposeCorrectly() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        ResourceHandle handle = (await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync()).Which;

        _importEnvironment.Release(handle).Should().BeOneOf(ReleaseStatus.Disposed, ReleaseStatus.NotDisposed);
        
        operation.Status.Should().Be(ImportingStatus.Disposed);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task Reimport_AfterDispose_ReinitializeImportOperation() {
        // Extension of Release_SingleTime_ShouldCancelImportingOperation
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();

        _importEnvironment.Release(PrebuildResourcesFixture.SimpleResourceStart).Should().BeOneOf(ReleaseStatus.Disposed, ReleaseStatus.NotDisposed);
        
        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.SimpleResourceStart);
        await new Func<Task<ResourceHandle>>(async () => await operation2).Should().NotThrowAsync();
        
        operation2.Status.Should().Be(ImportingStatus.Success);
        operation2.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }
}
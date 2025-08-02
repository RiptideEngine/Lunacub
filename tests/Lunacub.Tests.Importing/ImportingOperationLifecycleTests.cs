using System.Collections.Concurrent;

namespace Caxivitual.Lunacub.Tests.Importing;

[Collection<PrebuildResourcesCollectionFixture>]
public class ImportingOperationLifecycleTests : IDisposable {
    private readonly ImportEnvironment _importEnvironment;
    
    public ImportingOperationLifecycleTests(PrebuildResourcesFixture fixture, ITestOutputHelper output) {
        _importEnvironment = fixture.CreateImportEnvironment();
        _importEnvironment.Logger = output.BuildLogger();
    }

    public void Dispose() {
        _importEnvironment.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ImportFromID_SingleTime_InitializesImportOperation() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task ImportFromID_MultipleTime_ReturnsSameContainer() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);
        
        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ImportFromID_MultipleTimeParallel_IncrementsReferenceCountCorrectly() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, state) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
            }
        });
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ImportFromName_SingleTime_InitializesImportOperation() {
        var operation = _importEnvironment.Import(nameof(DeferrableResource));

        operation.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task ImportFromName_MultipleTime_ReturnsSameContainer() {
        var operation = _importEnvironment.Import(nameof(DeferrableResource));
        operation.Status.Should().Be(ImportingStatus.Importing);
        
        var operation2 = _importEnvironment.Import(nameof(DeferrableResource));
        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ImportFromName_MultipleTimeParallel_IncrementsReferenceCountCorrectly() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(nameof(DeferrableResource));
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, state) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Import(nameof(DeferrableResource));
            }
        });
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task ImportFromNameAndID_SameElement_ReturnsSameContainer() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);
        
        var operation2 = _importEnvironment.Import(nameof(DeferrableResource));
        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ImportFromNameAndID_SameElement_IncrementsReferenceCount() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);
        
        var operation2 = _importEnvironment.Import(nameof(DeferrableResource));
        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReleaseByOperation_SingleTime_ShouldCancelImportingOperation() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
        
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public async Task ReleaseByOperation_SingleTimeAfterImportMultipleTime_DecrementsReference() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
        
        _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Success);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        operation.Status.Should().Be(ImportingStatus.Importing);

        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ReleaseByOperation_MultipleTimeParallel_ShouldDecrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
            }
        });
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);
        
        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Success);
            }
        });
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ReleaseByID_SingleTime_ShouldCancelImportingOperation() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        _importEnvironment.Release(operation.Address).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
        
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public async Task ReleaseByID_SingleTimeAfterImportMultipleTime_DecrementsReference() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
        
        _importEnvironment.Release(operation.Address).Should().Be(ReleaseStatus.Success);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        operation.Status.Should().Be(ImportingStatus.Importing);

        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ReleaseByID_MultipleTimeParallel_ShouldDecrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
            }
        });
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);
        
        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Release(operation.Address).Should().Be(ReleaseStatus.Success);
            }
        });
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Dispose_ImportSingleTime_ShouldCancelOperation() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        _importEnvironment.Dispose();
        
        // Just ensure this doesn't lock.
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task Dispose_ImportMultipleTimes_ShouldCancelOperation() {
        ImportingOperation operation;

        for (int i = 0; i < 100; i++) {
            operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        }

        operation.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(100);
        
        _importEnvironment.Dispose();
        
        // Just ensure this doesn't lock.
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Reimport_AfterCancel_ReinitializeImportOperation() {
        // Extension of Release_SingleTime_ShouldCancelImportingOperation
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
        
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
        
        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        
        operation2.Status.Should().Be(ImportingStatus.Importing);
        operation2.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        // Just ensure this doesn't lock.
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
        
        operation2.Status.Should().Be(ImportingStatus.Success);
        operation2.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }
}
using System.Collections.Concurrent;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportingOperationLifecycleTests : IDisposable, IClassFixture<PrebuildResourcesFixture> {
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
    public async Task Import_SingleTime_ShouldHaveReferenceCountOf1() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.UnderlyingContainer.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Import_MultipleTimeParallel_ShouldIncrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.UnderlyingContainer.Status.Should().Be(ImportingStatus.Importing);

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
    public async Task Release_SingleTime_ShouldCancelImportingOperation() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
        
        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public async Task Release_SingleTimeAfterImportMultipleTime_ShouldDecrementReferenceButKeepOperationAlive() {
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        var operation2 = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
        
        _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Success);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task Release_MultipleTimeParallel_ShouldDecrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        
        var operation = _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
        operation.UnderlyingContainer.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, state) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Import(PrebuildResourcesFixture.DeferrableResource);
            }
        });
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);
        
        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, state) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                _importEnvironment.Release(operation).Should().Be(ReleaseStatus.Success);
            }
        });
        
        ((DeferrableResourceDeserializer)_importEnvironment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();
    }
}
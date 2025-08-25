// ReSharper disable AccessToDisposedClosure

using Microsoft.IO;
using System.Collections.Concurrent;
using FileSourceProvider = Caxivitual.Lunacub.Building.Core.FileSourceProvider;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceContainerCachingTests : IClassFixture<ComponentsFixture> {
    private readonly ComponentsFixture _componentsFixture;
    private readonly ITestOutputHelper _output;

    public ResourceContainerCachingTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _componentsFixture = componentsFixture;
        _output = output;
    }

    private ImportEnvironment BuildResources(Action<BuildResourceLibrary> libraryResourceAppender) {
        BuildResourceLibrary library = new(1, new FileSourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources")));
        libraryResourceAppender(library);

        MemoryOutputSystem memoryOutput = new();
        RecyclableMemoryStreamManager memStreamManager = new();
        
        using var environment = new BuildEnvironment(memoryOutput, memStreamManager)
            .AddLibrary(library);

        _componentsFixture.ApplyComponents(environment);
        environment.BuildResources();

        var importSourceProvider = new ImportMemorySourceProvider();
        var importLibrary = new ImportResourceLibrary(1, importSourceProvider);

        foreach ((var resourceId, var compiledBinary) in memoryOutput.Outputs[1].CompiledResources) {
            importSourceProvider.Resources.Add(resourceId, compiledBinary.Item1);
        }

        foreach ((var resourceId, var registryElement) in memoryOutput.Outputs[1].Registry) {
            importLibrary.AddRegistryElement(resourceId, registryElement);
        }

        ImportEnvironment importEnvironment = new ImportEnvironment(memStreamManager)
            .SetLogger(_output.BuildLogger())
            .AddLibrary(importLibrary);

        _componentsFixture.ApplyComponents(importEnvironment);

        return importEnvironment;
    }

    private ImportEnvironment BuildSingleResource() {
        return BuildResources(library => {
            library.AddRegistryElement(1, new(nameof(SimpleResource), [], new() {
                Addresses = new("SingleValue1.json"),
                Options = new(nameof(SimpleResourceImporter)),
            }));
        });
    }

    private ImportEnvironment BuildDeferrableResource() {
        return BuildResources(library => {
            library.AddRegistryElement(1, new(nameof(DeferrableResource), [], new() {
                Addresses = new("Empty.json"),
                Options = new(nameof(DeferrableResourceImporter)),
            }));
        });
    }

    [Fact]
    public void ImportFromIdAddress_SingleTime_InitializesImportOperation() {
        using ImportEnvironment environment = BuildSingleResource();

        var operation = environment.Import(1, 1);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }

    [Fact]
    public void ImportFromIdAddress_MultipleTime_ReturnsSameContainerAndIncrementsReference() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, 1);
        var operation2 = environment.Import(1, 1);

        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }

    [Fact]
    public void ImportFromIdAddress_MultipleTimeParallel_IncrementsReferenceAtomically() {
        const int count = 10000;
        using ImportEnvironment environment = BuildSingleResource();

        ImportingOperation operation;

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                operation = environment.Import(1, 1);
            }
        });

        operation.UnderlyingContainer.ReferenceCount.Should().Be(count);
    }

    [Fact]
    public void ImportFromNameAddress_SingleTime_InitializesImportOperation() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, nameof(SimpleResource));

        operation.Status.Should().Be(ImportingStatus.Importing);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }

    [Fact]
    public void ImportFromNameAddress_MultipleTime_ReturnsSameContainerAndIncrementsReference() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, nameof(SimpleResource));
        var operation2 = environment.Import(1, nameof(SimpleResource));
        
        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }

    [Fact]
    public void ImportFromNameAddress_MultipleTimeParallel_IncrementsReferenceAtomically() {
        const int count = 10000;
        using ImportEnvironment environment = BuildSingleResource();

        ImportingOperation operation;

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                operation = environment.Import(1, nameof(SimpleResource));
            }
        });

        operation.UnderlyingContainer.ReferenceCount.Should().Be(count);
    }

    [Fact]
    public void ImportFromNameAddressAndIdAddress_SameElement_ReturnsSameContainerAndIncrementsReference() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, 1);
        operation.Status.Should().Be(ImportingStatus.Importing);

        var operation2 = environment.Import(1, nameof(SimpleResource));
        operation2.UnderlyingContainer.Should().BeSameAs(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }

    [Fact]
    public void ImportFromTags_SingleTimeSingleTag_InitializesImportOperation() {
        using ImportEnvironment environment = BuildResources(library => {
            library.AddRegistryElement(1, new(nameof(SimpleResource), ["A", "B"], new() {
                Addresses = new("SingleValue1.json"),
                Options = new(nameof(SimpleResourceImporter)),
            }));
        });

        var operations = new Func<IReadOnlyCollection<ImportingOperation>>(() => environment.Import(new TagQuery("A"))).Should().NotThrow().Which;
        ImportingOperation operation = operations.Should().ContainSingle().Which;
        
        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }
    
    [Fact]
    public void ImportFromTags_MultipleTimeSingleTag_InitializesImportOperation() {
        using ImportEnvironment environment = BuildResources(library => {
            library.AddRegistryElement(1, new(nameof(SimpleResource), ["A", "B"], new() {
                Addresses = new("SingleValue1.json"),
                Options = new(nameof(SimpleResourceImporter)),
            }));
        });

        var operations = new Func<IReadOnlyCollection<ImportingOperation>>(() => environment.Import(new TagQuery("A"))).Should().NotThrow().Which;
        ImportingOperation operation = operations.Should().ContainSingle().Which;
        
        var operations2 = new Func<IReadOnlyCollection<ImportingOperation>>(() => environment.Import(new TagQuery("A"))).Should().NotThrow().Which;
        ImportingOperation operation2 = operations2.Should().ContainSingle().Which;

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }
    
    [Fact]
    public void ImportFromTags_MultipleTimeParallelSingleTag_InitializesImportOperation() {
        const int count = 10000;

        TagQuery query = new("A");
        
        using ImportEnvironment environment = BuildResources(library => {
            library.AddRegistryElement(1, new(nameof(SimpleResource), ["A", "B"], new() {
                Addresses = new("SingleValue1.json"),
                Options = new(nameof(SimpleResourceImporter)),
            }));
        });

        ImportingOperation operation;
        
        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                operation = environment.Import(query).Should().ContainSingle().Which;
            }
        });

        operation.UnderlyingContainer.ReferenceCount.Should().Be(count);
    }

    [Fact]
    public async Task ReleaseByOperation_SingleTime_CancelsImportingOperation() {
        using ImportEnvironment environment = BuildDeferrableResource();
        
        var operation = environment.Import(1, 1);
        environment.Release(operation).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();

        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public void ReleaseByOperation_SingleTimeAfterImportMultipleTime_DecrementsReference() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, 1);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        var operation2 = environment.Import(1, 1);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);

        environment.Release(operation).Should().Be(ReleaseStatus.Success);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        operation.Status.Should().Be(ImportingStatus.Importing);
    }

    [Fact]
    public void ReleaseByOperation_MultipleTimeParallel_ShouldDecrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        using ImportEnvironment environment = BuildSingleResource();

        var operation = environment.Import(1, 1);
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                environment.Import(1, 1);
            }
        });

        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                environment.Release(operation).Should().Be(ReleaseStatus.Success);
            }
        });
    }

    [Fact]
    public async Task ReleaseByID_SingleTime_ShouldCancelImportingOperation() {
        using ImportEnvironment environment = BuildDeferrableResource();
        
        var operation = environment.Import(1, 1);
        environment.Release(operation.Address).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();

        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    }

    [Fact]
    public void ReleaseByID_SingleTimeAfterImportMultipleTime_DecrementsReference() {
        using ImportEnvironment environment = BuildSingleResource();
        
        var operation = environment.Import(1, 1);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        var operation2 = environment.Import(1, 1);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);

        environment.Release(operation.Address).Should().Be(ReleaseStatus.Success);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
        operation.Status.Should().Be(ImportingStatus.Importing);
    }

    [Fact]
    public void ReleaseByID_MultipleTimeParallel_ShouldDecrementReferenceCountWithoutRaceCondition() {
        const int count = 10000;
        using ImportEnvironment environment = BuildSingleResource();

        var operation = environment.Import(1, 1);
        operation.Status.Should().Be(ImportingStatus.Importing);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                environment.Import(1, 1);
            }
        });

        operation.UnderlyingContainer.ReferenceCount.Should().Be(count + 1);

        Parallel.ForEach(Partitioner.Create(0, count, count / 10), (range, _) => {
            for (int i = range.Item1; i < range.Item2; i++) {
                environment.Release(operation.Address).Should().Be(ReleaseStatus.Success);
            }
        });
    }

    // [Fact]
    // public async Task Dispose_ImportSingleTime_ShouldCancelOperation() {
    //     using ImportEnvironment environment = BuildSingleResource();
    //     
    //     var operation = environment.Import(1, 1);
    //
    //     operation.Status.Should().Be(ImportingStatus.Importing);
    //     operation.UnderlyingContainer.ReferenceCount.Should().Be(1);
    //
    //     await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
    //     operation.Status.Should().Be(ImportingStatus.Canceled);
    //     operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
    // }
    //
    // [Fact]
    // public async Task Dispose_ImportMultipleTimes_ShouldCancelOperation() {
    //     ImportingOperation operation;
    //
    //     for (int i = 0; i < 100; i++) {
    //         operation = environment.Import(1, 1);
    //     }
    //
    //     operation.Status.Should().Be(ImportingStatus.Importing);
    //     operation.UnderlyingContainer.ReferenceCount.Should().Be(100);
    //
    //     environment.Dispose();
    //
    //     // Just ensure this doesn't lock.
    //     ((SimpleResourceDeserializer)environment.Deserializers[nameof(SimpleResourceDeserializer)]).Signal();
    //     await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();
    // }

    [Fact]
    public async Task Reimport_AfterCancel_ReinitializeImportOperation() {
        using ImportEnvironment environment = BuildDeferrableResource();

        var operation = environment.Import(1, 1);
        environment.Release(operation).Should().Be(ReleaseStatus.Canceled);

        await new Func<Task<ResourceHandle>>(async () => await operation).Should().ThrowAsync<OperationCanceledException>();

        operation.Status.Should().Be(ImportingStatus.Canceled);
        operation.UnderlyingContainer.ReferenceCount.Should().Be(0);

        var operation2 = environment.Import(1, 1);

        operation2.Status.Should().Be(ImportingStatus.Importing);
        operation2.UnderlyingContainer.ReferenceCount.Should().Be(1);
        
        ((DeferrableResourceDeserializer)environment.Deserializers[nameof(DeferrableResourceDeserializer)]).Signal();
        await new Func<Task<ResourceHandle>>(async () => await operation).Should().NotThrowAsync();

        operation2.Status.Should().Be(ImportingStatus.Success);
        operation2.UnderlyingContainer.ReferenceCount.Should().Be(1);
    }
}
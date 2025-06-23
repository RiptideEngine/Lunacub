namespace Caxivitual.Lunacub.Tests.Importing;

public class ReferenceCountTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ImportEnvironment _environment;
    
    public ReferenceCountTests(ComponentsFixture componentsFixture) {
        MockFileSystem _fileSystem = new();
        using BuildEnvironment buildEnvironment = new(new MockOutputSystem(_fileSystem)) {
            Resources = {
                [1] = new("Resource", [], new() {
                    Provider = new MemoryResourceProvider("{}"u8, DateTime.MinValue),
                    Options = new() {
                        ImporterName = nameof(DisposableResourceImporter),
                    },
                }),
            },
        };
        
        componentsFixture.ApplyComponents(buildEnvironment);

        buildEnvironment.BuildResources();

        _environment = new() {
            Libraries = {
                new MockResourceLibrary(_fileSystem),
            },
        };
        componentsFixture.ApplyComponents(_environment);
    }

    public void Dispose() {
        _environment.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    ~ReferenceCountTests() {
        _environment.Dispose();
    }

    [Fact]
    public async Task Import_Once_IncrementsRefCountCorrectly() {
        await _environment.Import<DisposableResource>(1);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(1);
    }
    
    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(100000)]
    public async Task Import_Parallel_IncrementsRefCountCorrectly(int times) {
        await Parallel.ForAsync(0, times, (_, _) => new(_environment.Import<DisposableResource>(1).Task));
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be((uint)times);
    }

    [Fact]
    public async Task ReleaseFromID_Once_DecrementsRefCountCorrectly() {
        await _environment.Import<DisposableResource>(1);
        await _environment.Import<DisposableResource>(1);
        await _environment.Import<DisposableResource>(1);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(3);

        _environment.Release(1).Should().Be(ReleaseStatus.Success);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(2);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ReleaseFromID_Parallel_DecrementsRefCountCorrectly(int times) {
        await Parallel.ForAsync(0, 10000, (_, _) => new(_environment.Import<DisposableResource>(1).Task));

        Parallel.For(0, times, (_, _) => _environment.Release(1));
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be((uint)(10000 - times));
    }
    
    [Fact]
    public async Task ReleaseFromHandle_Once_DecrementsRefCountCorrectly() {
        await _environment.Import<DisposableResource>(1);
        await _environment.Import<DisposableResource>(1);
        var handle = await _environment.Import<DisposableResource>(1);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(3);

        _environment.Release(handle).Should().Be(ReleaseStatus.Success);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(2);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ReleaseFromHandle_Parallel_DecrementsRefCountCorrectly(int times) {
        await Parallel.ForAsync(0, 9999, (_, _) => new(_environment.Import<DisposableResource>(1).Task));
        ResourceHandle<DisposableResource> handle = await _environment.Import<DisposableResource>(1).Task;
        
        Parallel.For(0, times, (_, _) => _environment.Release(handle));
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be((uint)(10000 - times));
    }
    
    [Fact]
    public async Task ReleaseFromObject_Once_DecrementsRefCountCorrectly() {
        await _environment.Import<DisposableResource>(1);
        await _environment.Import<DisposableResource>(1);
        var obj = (await _environment.Import<DisposableResource>(1)).Value!;
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(3);

        _environment.Release(obj).Should().Be(ReleaseStatus.Success);
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be(2);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ReleaseFromObject_Parallel_DecrementsRefCountCorrectly(int times) {
        await Parallel.ForAsync(0, 9999, (_, _) => new(_environment.Import<DisposableResource>(1).Task));
        DisposableResource obj = (await _environment.Import<DisposableResource>(1).Task).Value!;
        
        Parallel.For(0, times, (_, _) => _environment.Release(obj));
        
        (await _environment.GetResourceReferenceCountAsync(1)).Should().Be((uint)(10000 - times));
    }
}
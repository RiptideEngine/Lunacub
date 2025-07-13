using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportFailureTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ComponentsFixture _componentsFixture;
    private readonly ITestOutputHelper _output;

    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnvironment;
    private readonly ImportEnvironment _importEnvironment;
    
    public ImportFailureTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _componentsFixture = componentsFixture;
        _output = output;
        
        _fileSystem = new();
        
        _buildEnvironment = new(new MockOutputSystem(_fileSystem));
        _importEnvironment = new() {
            Logger = _output.BuildLogger(),
        };
        
        _componentsFixture.ApplyComponents(_buildEnvironment);
        _componentsFixture.ApplyComponents(_importEnvironment);
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        _buildEnvironment.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    [Fact]
    public async Task FailureImport_UnregisteredResource_ShouldReturnCorrectStates() {
        _importEnvironment.Libraries.Add(new(new Lunacub.Importing.Core.MemorySourceProvider()));

        var operation = _importEnvironment.Import(1);
        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<ArgumentException>().WithMessage("*unregistered*");

        operation.Status.Should().Be(ImportingStatus.Failed);

        _importEnvironment.Statistics.RemainReferenceCount.Should().Be(0);
        _importEnvironment.Statistics.TotalReferenceCount.Should().Be(0);
        _importEnvironment.Statistics.DisposedResourceCount.Should().Be(0);
        _importEnvironment.Statistics.UndisposedResourceCount.Should().Be(0);
        _importEnvironment.Statistics.UniqueResourceCount.Should().Be(0);
    }
    
    [Fact]
    public async Task FailureImport_NullResourceStream_ShouldReturnCorrectStates() {
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":69}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });
    
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new NullStreamSourceProvider()) {
            Registry = {
                [1] = new("Resource", [])
            },
        });

        var operation = _importEnvironment.Import(1);
        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<InvalidOperationException>().WithMessage("*null*stream*");
        
        operation.Status.Should().Be(ImportingStatus.Failed);
    }
}
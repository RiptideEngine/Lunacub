namespace Caxivitual.Lunacub.Tests.Building;

public class IncrementalBuildTests : IClassFixture<ComponentsFixture>, IClassFixture<MemoryStreamManagerFixture>, IDisposable {
    private readonly BuildEnvironment _environment;
    private readonly ComponentsFixture _componentsFixture;

    public IncrementalBuildTests(ComponentsFixture componentsFixture, MemoryStreamManagerFixture memoryStreamManagerFixture) {
        _componentsFixture = componentsFixture;

        _environment = new(new MemoryOutputSystem(), memoryStreamManagerFixture.Manager);
        
        _componentsFixture.ApplyComponents(_environment);
    }
    
    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }
    
    ~IncrementalBuildTests() {
        _environment.Dispose();
    }
    
    [Fact]
    public void BuildSimpleResources_MultipleTimes_BuildOnce() {
        _environment.Libraries.Add(new(1, new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow()
            .Which.EnvironmentResults.Should().ContainSingle()
            .Which.Value.Should().ContainSingle()
            .Which.Value.Status.Should().Be(BuildStatus.Success);
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow()
            .Which.EnvironmentResults.Should().ContainSingle()
            .Which.Value.Should().ContainSingle()
            .Which.Value.Status.Should().Be(BuildStatus.Cached);
    }

    [Fact]
    public void BuildResources_RemoveIncrementalInfo_ShouldRebuild() {
        _environment.Libraries.Add(new(1, new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });
        
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow()
            .Which.EnvironmentResults.Should().ContainSingle()
            .Which.Value.Should().ContainSingle()
            .Which.Value.Status.Should().Be(BuildStatus.Success);
        
        _environment.IncrementalInfos.Remove(1);
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow()
            .Which.EnvironmentResults.Should().ContainSingle()
            .Which.Value.Should().ContainSingle()
            .Which.Value.Status.Should().Be(BuildStatus.Success);
    }
}
namespace Caxivitual.Lunacub.Tests.Building;

public class IncrementalBuildTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly BuildEnvironment _environment;
    private readonly ComponentsFixture _componentsFixture;

    public IncrementalBuildTests(ComponentsFixture componentsFixture) {
        _componentsFixture = componentsFixture;

        _environment = new(new MockOutputSystem());
        
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
    public void BuildResources_Normal_ShouldNotRebuild() {
        _environment.Resources.Add(1, new("Resource", [], new() {
            Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithValueImporter),
            },
        }));
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which.Value.Status.Should().Be(BuildStatus.Success);
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which.Value.Status.Should().Be(BuildStatus.Cached);
    }

    [Fact]
    public void BuildResources_RemoveIncrementalInfo_ShouldRebuild() {
        _environment.Resources.Add(1, new("Resource", [], new() {
            Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithValueImporter),
            },
        }));
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which.Value.Status.Should().Be(BuildStatus.Success);
        _environment.IncrementalInfos.Remove(1);
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which.Value.Status.Should().Be(BuildStatus.Success);
    }
}
namespace Caxivitual.Lunacub.Tests.Building;

public sealed class BuildCoverageTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ITestOutputHelper _output;
    private readonly BuildEnvironment _environment;

    public BuildCoverageTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _output = output;
        DebugHelpers.RedirectConsoleOutput(output);
        
        _environment = new(new MockOutputSystem());
        componentsFixture.ApplyComponents(_environment);
    }

    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }

    ~BuildCoverageTests() {
        _environment.Dispose();
    }
    
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void BuildResources_SimpleResource_BuildEverything(int loop) {
        for (int i = 0; i < loop; i++) {
            _output.WriteLine(new ResourceID((UInt128)(1 + i)).ToString());
            
            _environment.Resources.Add((UInt128)(1 + i), new() {
                Provider = new MemoryResourceProvider("""{"Value":0}"""u8, DateTime.MinValue),
                Options = new() {
                    ImporterName = nameof(SimpleResourceImporter),
                },
            });
        }

        var result = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(loop);
    }

    [Fact]
    public void BuildResource_SimpleResource_OnlyBuildRequested() {
        _environment.Resources.Add(1, new() {
            Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(SimpleResourceImporter),
            },
        });
        _environment.Resources.Add(2, new() {
            Provider = new MemoryResourceProvider("""{"Value":2}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(SimpleResourceImporter),
            },
        });

        new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();
    }

    [Fact]
    public void BuildResource_ReferenceResource_BuildsWithDependencies() {
        _environment.Resources.Add(1, new() {
            Provider = new MemoryResourceProvider("""{"Reference":2,"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferenceResourceImporter),
            },
        });
        _environment.Resources.Add(2, new() {
            Provider = new MemoryResourceProvider("""{"Reference":0,"Value":2}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferenceResourceImporter),
            },
        });
        _environment.Resources.Add(3, new() {
            Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(SimpleResourceImporter),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().HaveCount(2);
    }
    
    [Fact]
    public void Build_UnregisteredDependencyCoverage_IgnoreUnregisteredDependencies() {
        _environment.Resources.Add(1, new() {
            Provider = new MemoryResourceProvider("""{"Reference":999,"Value":1}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferenceResourceImporter),
            },
        });
        _environment.Resources.Add(2, new() {
            Provider = new MemoryResourceProvider("""{"Reference":0,"Value":2}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferenceResourceImporter),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();
    }
}
using System.Text;

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
                    ImporterName = nameof(ResourceWithValueImporter),
                },
            });
        }

        var result = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(loop);
    }
    
    [Fact]
    public void BuildResources_ChainDependency_BuildEverything() {
        RegisterDependencyResource(1, 1, 2);
        RegisterDependencyResource(2, 2, 3);
        RegisterDependencyResource(3, 3);

        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;
        
        resourceBuildResults.Should().HaveCount(3);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().Equal(3);
        _environment.IncrementalInfos.Should().ContainKey(3).WhoseValue.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void BuildResources_DiamondDependency_BuildCorrectly() {
        RegisterDependencyResource(1, 1, 2, 3);
        RegisterDependencyResource(2, 2, 4);
        RegisterDependencyResource(3, 3, 4);
        RegisterDependencyResource(4, 4);
        
        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;
        
        resourceBuildResults.Should().HaveCount(4);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2, 3);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().Equal(4);
        _environment.IncrementalInfos.Should().ContainKey(3).WhoseValue.Dependencies.Should().Equal(4);
        _environment.IncrementalInfos.Should().ContainKey(4).WhoseValue.Dependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void BuildResources_TriangleDependency_BuildCorrectly() {
        RegisterDependencyResource(1, 1, 2, 3);
        RegisterDependencyResource(2, 2, 3);
        RegisterDependencyResource(3, 3);
        
        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;
        
        resourceBuildResults.Should().HaveCount(3);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2, 3);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().Equal(3);
        _environment.IncrementalInfos.Should().ContainKey(3).WhoseValue.Dependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void BuildResources_MultipleDependencyRoot_BuildCorrectly() {
        /*
                       7
                       |
                1      4
              /   \  /   \ 
            2      3      5
                   |
                   6
         */
        
        RegisterDependencyResource(1, 1, 2, 3);
        RegisterDependencyResource(2, 2);
        RegisterDependencyResource(3, 3, 6);
        RegisterDependencyResource(4, 4, 3, 5);
        RegisterDependencyResource(5, 5);
        RegisterDependencyResource(6, 6);
        RegisterDependencyResource(7, 7, 4);
        
        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;
        
        resourceBuildResults.Should().HaveCount(7);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2, 3);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().BeEmpty();
        _environment.IncrementalInfos.Should().ContainKey(3).WhoseValue.Dependencies.Should().Equal(6);
        _environment.IncrementalInfos.Should().ContainKey(4).WhoseValue.Dependencies.Should().Equal(3, 5);
        _environment.IncrementalInfos.Should().ContainKey(5).WhoseValue.Dependencies.Should().BeEmpty();
        _environment.IncrementalInfos.Should().ContainKey(6).WhoseValue.Dependencies.Should().BeEmpty();
        _environment.IncrementalInfos.Should().ContainKey(7).WhoseValue.Dependencies.Should().Equal(4);
    }

    [Fact]
    public void BuildResources_MissingDependency_IgnoreMissingDependencies() {
        RegisterDependencyResource(1, 1, 2, 3);
        RegisterDependencyResource(2, 2);
        
        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;

        resourceBuildResults.Should().HaveCount(2);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void BuildResources_CircularDependency_ThrowsException() {
        RegisterDependencyResource(1, 1, 2);
        RegisterDependencyResource(2, 2, 1);

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().Throw<Exception>().WithMessage("*ircular dependency detected*");
    }
    
    [Fact]
    public void BuildResources_SelfDependent_ShouldBeIgnored() {
        RegisterDependencyResource(1, 1, 1, 2);
        RegisterDependencyResource(2, 2);

        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;

        resourceBuildResults.Should().HaveCount(2);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void BuildResources_WithDependencies_BuildEverything() {
        RegisterDependencyResource(1, 1, 2);
        RegisterDependencyResource(2, 2);
        _environment.Resources.Add(3, new() {
            Provider = new MemoryResourceProvider("""{"Value":3}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithValueImporter),
            },
        });
        _environment.Resources.Add(4, new() {
            Provider = new MemoryResourceProvider("""{"Value":4}"""u8, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithValueImporter),
            },
        });
        
        var buildResult = new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which;
        var resourceBuildResults = buildResult.ResourceResults;

        resourceBuildResults.Should().HaveCount(4);
        _environment.IncrementalInfos.Should().ContainKey(1).WhoseValue.Dependencies.Should().Equal(2);
        _environment.IncrementalInfos.Should().ContainKey(2).WhoseValue.Dependencies.Should().BeEmpty();
        _environment.IncrementalInfos.Should().ContainKey(3).WhoseValue.Dependencies.Should().BeEmpty();
        _environment.IncrementalInfos.Should().ContainKey(4).WhoseValue.Dependencies.Should().BeEmpty();
    }

    private void RegisterDependencyResource(ResourceID id, int value, params ResourceID[] dependencies) {
        byte[] data = Encoding.UTF8.GetBytes($$"""{"Value":{{value}},"Dependencies":[{{string.Join(',', dependencies)}}]}""");
        
        _environment.Resources.Add(id, new() {
            Provider = new MemoryResourceProvider(data, DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithDependenciesImporter),
            },
        });
    }

    // [Fact]
    // public void BuildResource_SimpleResource_OnlyBuildRequested() {
    //     _environment.Resources.Add(1, new() {
    //         Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithValueImporter),
    //         },
    //     });
    //     _environment.Resources.Add(2, new() {
    //         Provider = new MemoryResourceProvider("""{"Value":2}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithValueImporter),
    //         },
    //     });
    //
    //     new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();
    // }
    //
    // [Fact]
    // public void BuildResource_ReferenceResource_BuildsWithDependencies() {
    //     _environment.Resources.Add(1, new() {
    //         Provider = new MemoryResourceProvider("""{"Reference":2,"Value":1}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithReferenceImporter),
    //         },
    //     });
    //     _environment.Resources.Add(2, new() {
    //         Provider = new MemoryResourceProvider("""{"Reference":0,"Value":2}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithReferenceImporter),
    //         },
    //     });
    //     _environment.Resources.Add(3, new() {
    //         Provider = new MemoryResourceProvider("""{"Value":1}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithValueImporter),
    //         },
    //     });
    //     
    //     new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().HaveCount(2);
    // }
    //
    // [Fact]
    // public void BuildResource_UnregisteredDependencyCoverage_IgnoreUnregisteredDependencies() {
    //     _environment.Resources.Add(1, new() {
    //         Provider = new MemoryResourceProvider("""{"Reference":999,"Value":1}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithReferenceImporter),
    //         },
    //     });
    //     _environment.Resources.Add(2, new() {
    //         Provider = new MemoryResourceProvider("""{"Reference":0,"Value":2}"""u8, DateTime.MinValue),
    //         Options = new() {
    //             ImporterName = nameof(ResourceWithReferenceImporter),
    //         },
    //     });
    //     
    //     new Func<BuildingResult>(() => _environment.BuildResource(1)).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();
    // }
}
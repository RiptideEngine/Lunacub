namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public async Task ImportOptionsResource_BinaryOption_ReturnsCorrectData() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ConfigurableResourceImporter),
                Options = new ConfigurableResourceDTO.Options(OutputType.Binary),
            },
        }));

        _buildEnvironment.BuildResources();
        
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var result = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
                .Should()
                .CompleteWithinAsync(0.5.Seconds(), "Simple resource shouldn't take this long."))
            .Subject;

        result.ResourceId.Should().Be((ResourceID)1);
        result.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }
    
    [Fact]
    public async Task ImportOptionsResource_JsonOption_ReturnsCorrectData() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ConfigurableResourceImporter),
                Options = new ConfigurableResourceDTO.Options(OutputType.Json),
            },
        }));

        _buildEnvironment.BuildResources();
        
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var result = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
                .Should()
                .CompleteWithinAsync(0.5.Seconds(), "Simple resource shouldn't take this long."))
            .Subject;

        result.ResourceId.Should().Be((ResourceID)1);
        result.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }
}
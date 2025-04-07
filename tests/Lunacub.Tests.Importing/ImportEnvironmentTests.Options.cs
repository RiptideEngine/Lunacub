namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void ImportOptionsResource_BinaryOption_ReturnsCorrectData() {
        ResourceID rid = ResourceID.Parse("178ad6eee6e4521f91c9668566a4b6eb");

        var result = BuildResources(rid);

        result.ResourceResults.Should().ContainKey(rid).WhoseValue.Status.Should().BeOneOf(BuildStatus.Success, BuildStatus.Cached);
        
        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));

        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid);
        handle.Value.Should().BeOfType<OptionsResource>().Which.Array.Should().Equal(Enumerable.Range(0, 10));
    }
    
    [Fact]
    public void ImportOptionsResource_JsonOption_ReturnsCorrectData() {
        ResourceID rid = ResourceID.Parse("81a12ccd19f15cd6a5df2513c95ffbd1");

        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid);
        handle.Value.Should().BeOfType<OptionsResource>().Which.Array.Should().Equal(Enumerable.Range(0, 10));
    }
}
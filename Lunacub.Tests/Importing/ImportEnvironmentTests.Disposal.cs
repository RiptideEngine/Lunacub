namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void DisposableResource_Import_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");

        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        new Func<object?>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Should().BeOfType<DisposableResource>();
    }
    
    [Fact]
    public void DisposableResource_Dispose_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");

        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        new Func<object?>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Should().BeOfType<DisposableResource>();
    }
}
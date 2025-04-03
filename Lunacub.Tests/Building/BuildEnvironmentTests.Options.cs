namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void OptionsResource_ShouldBeCorrect() {
        var rid = ResourceID.Parse("07c0f1c6842b5500a5a5481e01ab4945");
        _context.Resources.Add(rid, GetResourcePath("SimpleResource.json"), new(nameof(SimpleResourceImporter), null));
        
        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;

        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which.
            ResourceResults.Should().ContainKey(rid).WhoseValue;

        result.Exception.Should().BeNull();
        result.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
    }
}
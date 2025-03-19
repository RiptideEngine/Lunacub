namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void SimpleResource_ShouldBeCorrect() {
        var rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        _context.Resources.Add(rid, GetResourcePath("SimpleResource.json"), new(nameof(SimpleResourceImporter), null));
        
        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;

        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which.
            ResourceResults.Should().ContainKey(rid).WhoseValue;
        
        result.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
    }
}
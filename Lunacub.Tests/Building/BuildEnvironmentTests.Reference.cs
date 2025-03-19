namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void ReferenceResource_BuildAll_ShouldBeCorrect() {
        var rid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
        _context.Resources.Add(rid1, GetResourcePath("ReferenceResource1.json"), new(nameof(ReferenceResourceImporter), null));
        
        var rid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
        _context.Resources.Add(rid2, GetResourcePath("ReferenceResource2.json"), new(nameof(ReferenceResourceImporter), null));
        
        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;

        var results = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which.ResourceResults;

        var result1 = results.Should().ContainKey(rid1).WhoseValue;
        
        result1.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var result2 = results.Should().ContainKey(rid2).WhoseValue;
        
        result2.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid2}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
    }
    
    [Fact]
    public void ReferenceResource_BuildSingle_ShouldBeCorrect() {
        var rid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
        _context.Resources.Add(rid1, GetResourcePath("ReferenceResource1.json"), new(nameof(ReferenceResourceImporter), null));
        
        var rid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
        _context.Resources.Add(rid2, GetResourcePath("ReferenceResource2.json"), new(nameof(ReferenceResourceImporter), null));
        
        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;

        var results = new Func<BuildingResult>(() => _context.BuildResource(rid1)).Should().NotThrow().Which.ResourceResults;

        var result1 = results.Should().ContainKey(rid1).WhoseValue;
        
        result1.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var result2 = results.Should().ContainKey(rid2).WhoseValue;
        
        result2.IsSuccess.Should().BeTrue();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid2}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
    }
}
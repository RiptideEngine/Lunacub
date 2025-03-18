using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void SimpleResource_ShouldBeCorrect() {
        var rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        _context.Resources.Add(rid, GetResourcePath("SimpleResource.json"), new(nameof(SimpleResourceImporter), null));
        
        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.IsSuccess.Should().BeTrue();

        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;
        
        var report = result.Reports.Should().ContainKey(rid).WhoseValue;
        report.Dependencies.Should().BeEmpty();
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
    }
}
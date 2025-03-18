using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void DependentResource_ShouldBeCorrect() {
        var dependentRid = ResourceID.Parse("c5a8758032c94f2fa06c6ec22901f6e7");
        var dependency1Rid = ResourceID.Parse("65609f8b1ae340769cfb6d4a38255fdc");
        
        _context.Resources.Add(dependentRid, GetResourcePath("DependentResource.json"), new(nameof(DependentResourceImporter), null));
        _context.Resources.Add(dependency1Rid, GetResourcePath("DependencyResource.json"), new(nameof(SimpleResourceImporter), null));
    
        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.Exception.Should().BeNull();
        result.IsSuccess.Should().BeTrue();

        MockFileSystem fs = ((MockOutputSystem)_context.Output).FileSystem;
    
        result.Reports.Should().HaveCount(2);

        string dependentPath = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{dependentRid}{CompilingConstants.CompiledResourceExtension}");
        string dependencyPath = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{dependentRid}{CompilingConstants.CompiledResourceExtension}");
        
        var dependentReport = result.Reports.Should().ContainKey(dependentRid).WhoseValue;
        dependentReport.Dependencies.Should().HaveCount(1).And.Contain(dependency1Rid);
        fs.File.Exists(dependentPath).Should().BeTrue();
        
        var dependencyReport = result.Reports.Should().ContainKey(dependency1Rid).WhoseValue;
        dependencyReport.Dependencies.Should().BeEmpty();
        fs.File.Exists(dependencyPath).Should().BeTrue();
    
        fs.File.GetLastWriteTime(dependencyPath).Should().BeBefore(fs.File.GetLastWriteTime(dependentPath));
    }
}
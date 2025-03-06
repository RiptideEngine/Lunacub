using System.IO.Abstractions;

namespace Caxivitual.Lunacub.Tests.Building;

public class BuildingContextTests : IDisposable {
    private readonly BuildingContext _context;
    private readonly ITestOutputHelper _output;

    public BuildingContextTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _context = new(new MockBuildOutput());
        
        _context.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _context.Serializers.Add(new SimpleResourceSerializer());
        
        _context.Importers.Add(nameof(DependentResourceImporter), new DependentResourceImporter());
        _context.Serializers.Add(new DependentResourceSerializer());
    }

    public void Dispose() {
        _context.Dispose();
    }

    [Fact]
    public void SimpleResourceShouldBeCorrect() {
        var rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        _context.Resources.Add(rid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SimpleResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        
        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.IsSuccess.Should().BeTrue();
        
        var report = result.Reports.Should().ContainKey(rid).WhoseValue;
        report.Dependencies.Should().BeEmpty();
        ((MockBuildOutput)_context.Output).FileSystem.File.Exists(report.DestinationPath).Should().BeTrue();
    }

    [Fact]
    public void DependentResourceShouldBeCorrect() {
        var dependentRid = ResourceID.Parse("c5a8758032c94f2fa06c6ec22901f6e7");
        var dependencyRid = ResourceID.Parse("65609f8b1ae340769cfb6d4a38255fdc");
        
        _context.Resources.Add(dependentRid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DependentResource.json"), new() {
            ImporterName = nameof(DependentResourceImporter),
            ProcessorName = null,
        });
        _context.Resources.Add(dependencyRid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DependencyResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });

        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.IsSuccess.Should().BeTrue();

        IFile file = ((MockBuildOutput)_context.Output).FileSystem.File;
        
        var dependentReport = result.Reports.Should().ContainKey(dependentRid).WhoseValue;
        dependentReport.Dependencies.Should().HaveCount(1).And.Contain(dependencyRid);
        file.Exists(dependentReport.DestinationPath).Should().BeTrue();
        
        var dependencyReport = result.Reports.Should().ContainKey(dependencyRid).WhoseValue;
        dependencyReport.Dependencies.Should().BeEmpty();
        file.Exists(dependencyReport.DestinationPath).Should().BeTrue();
        
        file.GetLastWriteTime(dependencyReport.DestinationPath).Should().BeBefore(file.GetLastWriteTime(dependentReport.DestinationPath));
    }
}
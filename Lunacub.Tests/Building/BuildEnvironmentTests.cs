using System.IO.Abstractions;

namespace Caxivitual.Lunacub.Tests.Building;

public class BuildEnvironmentTests : IDisposable {
    private readonly BuildEnvironment _context;
    private readonly ITestOutputHelper _output;

    public BuildEnvironmentTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _context = new(new MockOutputSystem());
        
        _context.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _context.Serializers.Add(new SimpleResourceSerializer());
        
        _context.Importers.Add(nameof(DependentResourceImporter), new DependentResourceImporter());
        _context.Serializers.Add(new DependentResourceSerializer());
        
        _context.Importers.Add(nameof(CircularDependentResource), new CircularDependentResourceImporter());
        _context.Serializers.Add(new CircularDependentResourceSerializer());
    }

    public void Dispose() {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);

    [Fact]
    public void SimpleResource_ShouldBeCorrect() {
        var rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        _context.Resources.Add(rid, GetResourcePath("SimpleResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        
        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.IsSuccess.Should().BeTrue();
        
        var report = result.Reports.Should().ContainKey(rid).WhoseValue;
        report.Dependencies.Should().BeEmpty();
        ((MockOutputSystem)_context.Output).FileSystem.File.Exists(report.DestinationPath).Should().BeTrue();
    }

    [Fact]
    public void DependentResource_ShouldBeCorrect() {
        var dependentRid = ResourceID.Parse("c5a8758032c94f2fa06c6ec22901f6e7");
        var dependency1Rid = ResourceID.Parse("65609f8b1ae340769cfb6d4a38255fdc");
        var dependency2Rid = ResourceID.Parse("eef73a9765e54e7cac79fadbea9b31e9");
        
        _context.Resources.Add(dependentRid, GetResourcePath("DependentResource.json"), new() {
            ImporterName = nameof(DependentResourceImporter),
            ProcessorName = null,
        });
        _context.Resources.Add(dependency1Rid, GetResourcePath("DependencyResource1.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        _context.Resources.Add(dependency2Rid, GetResourcePath("DependencyResource2.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });

        var result = new Func<BuildingResult>(() => _context.BuildResources()).Should().NotThrow().Which;
        result.IsSuccess.Should().BeTrue();

        IFile file = ((MockOutputSystem)_context.Output).FileSystem.File;

        result.Reports.Should().HaveCount(3);
        
        var dependentReport = result.Reports.Should().ContainKey(dependentRid).WhoseValue;
        dependentReport.Dependencies.Should().HaveCount(2).And.Contain(dependency1Rid).And.Contain(dependency2Rid);
        file.Exists(dependentReport.DestinationPath).Should().BeTrue();
        
        var dependency1Report = result.Reports.Should().ContainKey(dependency1Rid).WhoseValue;
        dependency1Report.Dependencies.Should().BeEmpty();
        file.Exists(dependency1Report.DestinationPath).Should().BeTrue();

        var dependency2Report = result.Reports.Should().ContainKey(dependency2Rid).WhoseValue;
        dependency2Report.Dependencies.Should().BeEmpty();
        file.Exists(dependency2Report.DestinationPath).Should().BeTrue();

        file.GetLastWriteTime(dependency1Report.DestinationPath).Should().BeBefore(file.GetLastWriteTime(dependentReport.DestinationPath));
        file.GetLastWriteTime(dependency2Report.DestinationPath).Should().BeBefore(file.GetLastWriteTime(dependentReport.DestinationPath));
    }

    [Fact]
    public void CircularDependentResource_ShouldBeCorrect() {
        var cdr1 = ResourceID.Parse("0195921b2ac17986b78abcb187f64dd2");
        var cdr2 = ResourceID.Parse("0195921aa48a7153906e0ecd9a7bdf33");
        
        _context.Resources.Add(cdr1, GetResourcePath("CircularDependentResource1.json"), new() {
            ImporterName = nameof(CircularDependentResource),
            ProcessorName = null,
        });
        _context.Resources.Add(cdr2, GetResourcePath("CircularDependentResource2.json"), new() {
            ImporterName = nameof(CircularDependentResource),
            ProcessorName = null,
        });

        var report = _context.BuildResources();
        report.Exception.Should().BeNull();
    }
}
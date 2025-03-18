using Caxivitual.Lunacub.Compilation;
using System.IO.Abstractions.TestingHelpers;

namespace Caxivitual.Lunacub.Tests;

internal sealed class MockOutputSystem : OutputSystem {
    public static string ReportDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
    public static string ResourceOutputDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs");

    public MockFileSystem FileSystem { get; }

    public MockOutputSystem(MockFileSystem? fs = null) {
        FileSystem = fs ?? new();
        
        FileSystem.Directory.CreateDirectory(ReportDirectory);
        FileSystem.Directory.CreateDirectory(ResourceOutputDirectory);
    }
    
    public override void CollectReports(IDictionary<ResourceID, BuildingReport> receiver) { }

    public override void FlushReports(IReadOnlyDictionary<ResourceID, BuildingReport> reports) {
        foreach ((var rid, var report) in reports) {
            using Stream reportFile = new MockFileStream(FileSystem, Path.Combine(ReportDirectory, $"{rid}{CompilingConstants.ReportExtension}"), FileMode.OpenOrCreate);
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
        }
    }
    
    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        string path = FileSystem.Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.GetLastWriteTime(path) : null;
    }

    public override Stream CreateDestinationStream(ResourceID rid) {
        return new MockFileStream(FileSystem, FileSystem.Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"), FileMode.Create);
    }
}
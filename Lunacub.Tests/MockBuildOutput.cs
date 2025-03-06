using Caxivitual.Lunacub.Compilation;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Caxivitual.Lunacub.Tests;

internal sealed class MockBuildOutput : BuildOutput {
    public static string ReportDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
    public static string ResourceOutputDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs");

    public MockFileSystem FileSystem { get; } = new();

    public MockBuildOutput() {
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

    public override string GetBuildDestination(ResourceID rid) => Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");

    public override Stream GetResourceOutputStream(string buildDestination) {
        MockFileStream stream = new(FileSystem, buildDestination, FileMode.OpenOrCreate);
        stream.SetLength(0);
        stream.Flush();

        return stream;
    }
}
namespace Caxivitual.Lunacub.Tests.Common;

public sealed class MockOutputSystem : OutputSystem {
    public static string ReportDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
    public static string ResourceOutputDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs");

    public MockFileSystem FileSystem { get; }

    public MockOutputSystem(MockFileSystem? fs = null) {
        FileSystem = fs ?? new();
        
        FileSystem.Directory.CreateDirectory(ReportDirectory);
        FileSystem.Directory.CreateDirectory(ResourceOutputDirectory);
    }
    
    public override void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver) { }

    public override void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports) {
        foreach ((var rid, var report) in reports) {
            using Stream reportFile = new MockFileStream(FileSystem, Path.Combine(ReportDirectory, $"{rid:X}{CompilingConstants.ReportExtension}"), FileMode.OpenOrCreate);
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
        }
    }
    
    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        string path = FileSystem.Path.Combine(ResourceOutputDirectory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}");
        
        return FileSystem.File.Exists(path) ? FileSystem.File.GetLastWriteTime(path) : null;
    }

    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        using MockFileStream outputStream = new(FileSystem, FileSystem.Path.Combine(ResourceOutputDirectory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}"), FileMode.Create);
        sourceStream.Position = 0;
        sourceStream.CopyTo(outputStream);
    }

    public override void OutputResourceRegistry(IReadOnlyDictionary<ResourceID, OutputRegistryElement> registry) {
        using MockFileStream stream = new(FileSystem, FileSystem.Path.Combine(ResourceOutputDirectory, "__registry"), FileMode.Create);

        JsonSerializer.Serialize(stream, registry);
    }
}
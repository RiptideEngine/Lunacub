using System.Text.Json;

namespace Caxivitual.Lunacub.Tests.Building;

public class EnvironmentTests {
    [Fact]
    public void Dispose_MemoryOutput_FlushIncrementalInfos() {
        DateTime primaryLastWriteTime = DateTime.Now;

        MemoryOutputSystem output = new();
        BuildEnvironment environment = new(output);
        environment.IncrementalInfos.Add(1, new(new(primaryLastWriteTime), new("Importer", "Processor"), new HashSet<ResourceID> {
            1, 2, 3,
        }, new("1.0", "1.0")));

        environment.Dispose();

        output.IncrementalInfos.Should().ContainSingle();
    }
    
    [Fact]
    public void Dispose_FileOutput_FlushIncrementalInfos() {
        DateTime primaryLastWriteTime = DateTime.Now;

        MockOutputSystem output = new();
        BuildEnvironment environment = new(output);
        environment.IncrementalInfos.Add(1, new(new(primaryLastWriteTime), new("Importer", "Processor"), new HashSet<ResourceID> {
            1, 2, 3,
        }, new("1.0", "1.0")));

        environment.Dispose();

        using Stream stream = new Func<Stream>(() => output.FileSystem.File.OpenRead(output.FileSystem.Path.Combine(MockOutputSystem.ReportDirectory, $"1{CompilingConstants.ReportExtension}"))).Should().NotThrow().Which;
        
        IncrementalInfo info = new Func<IncrementalInfo>(() => JsonSerializer.Deserialize<IncrementalInfo>(stream!)).Should().NotThrow().Which;
        
        info.SourcesLastWriteTime.Primary.Should().Be(primaryLastWriteTime);
        info.SourcesLastWriteTime.Secondaries.Should().BeNullOrEmpty();

        info.Options.Should().BeEquivalentTo(new {
            ImporterName = "Importer",
            ProcessorName = "Processor",
            Options = (IImportOptions?)null,
        });

        info.DependencyAddresses.Should().BeEquivalentTo(new ResourceID[] { 1, 2, 3 });

        info.ComponentVersions.Should().Be(new ComponentVersions("1.0", "1.0"));
    }
}
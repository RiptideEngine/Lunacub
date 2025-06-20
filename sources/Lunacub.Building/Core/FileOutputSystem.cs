using System.Globalization;

namespace Caxivitual.Lunacub.Building.Core;

[ExcludeFromCodeCoverage]
public class FileOutputSystem : OutputSystem {
    public string ReportDirectory { get; }
    public string ResourceOutputDirectory { get; }
    
    public FileOutputSystem(string reportDirectory, string resourceOutputDirectory) {
        ReportDirectory = Path.GetFullPath(reportDirectory);
        if (!Directory.Exists(ReportDirectory)) {
            throw new ArgumentException($"Report directory '{ReportDirectory}' does not exist.");
        }
        
        ResourceOutputDirectory = Path.GetFullPath(resourceOutputDirectory);
        if (!Directory.Exists(ResourceOutputDirectory)) {
            throw new ArgumentException($"Resource output directory '{ResourceOutputDirectory}' does not exist.");
        }
    }

    public override void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver) {
        foreach (var file in Directory.EnumerateFiles(ReportDirectory, $"*{CompilingConstants.ReportExtension}", SearchOption.TopDirectoryOnly)) {
            if (!ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, null, out var rid)) continue;

            try {
                using FileStream reportFile = File.OpenRead(file);
                
                receiver.Add(rid, JsonSerializer.Deserialize<IncrementalInfo>(reportFile));
            } catch (Exception e) {
                // Ignore any failed attempt to deserialize report.
            }
        }
    }

    public override void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports) {
        foreach ((var rid, var report) in reports) {
            using FileStream reportFile = File.OpenWrite(Path.Combine(ReportDirectory, $"{rid:X}{CompilingConstants.ReportExtension}"));
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
        }
    }

    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        string path = Path.Combine(ResourceOutputDirectory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.GetLastWriteTime(path) : null;
    }

    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        using FileStream fs = new(Path.Combine(ResourceOutputDirectory, $"{rid:X}{CompilingConstants.CompiledResourceExtension}"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        fs.SetLength(0);
        fs.Flush();
        
        sourceStream.CopyTo(fs);
    }

    public override void OutputResourceRegistry(IReadOnlyDictionary<ResourceID, OutputRegistryElement> registry) {
        using FileStream fs = new(Path.Combine(ResourceOutputDirectory, "__registry"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        fs.SetLength(0);
        fs.Flush();

        JsonSerializer.Serialize(fs, registry);
    }
}
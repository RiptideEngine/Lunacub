namespace Caxivitual.Lunacub.Building;

public class FileBuildOutput : BuildOutput {
    public string ReportDirectory { get; }
    public string ResourceOutputDirectory { get; }
    
    public FileBuildOutput(string reportDirectory, string resourceOutputDirectory) {
        ReportDirectory = Path.GetFullPath(reportDirectory);
        if (!Directory.Exists(ReportDirectory)) {
            throw new ArgumentException($"Report directory '{ReportDirectory}' does not exist.");
        }
        
        ResourceOutputDirectory = Path.GetFullPath(resourceOutputDirectory);
        if (!Directory.Exists(ResourceOutputDirectory)) {
            throw new ArgumentException($"Resource output directory '{ResourceOutputDirectory}' does not exist.");
        }
    }

    public override void CollectReports(IDictionary<ResourceID, BuildingReport> receiver) {
        foreach (var file in Directory.EnumerateFiles(ReportDirectory, $"*{CompilingConstants.ReportExtension}", SearchOption.TopDirectoryOnly)) {
            if (!ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), out var rid)) continue;

            try {
                using FileStream reportFile = File.OpenRead(file);
                
                receiver.Add(rid, JsonSerializer.Deserialize<BuildingReport>(reportFile));
            } catch {
                // Ignore any failed attempt to deserialize report.
            }
        }
    }

    public override void FlushReports(IReadOnlyDictionary<ResourceID, BuildingReport> reports) {
        foreach ((var rid, var report) in reports) {
            using FileStream reportFile = File.OpenWrite(Path.Combine(ReportDirectory, $"{rid}{CompilingConstants.ReportExtension}"));
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
        }
    }

    public override string GetBuildDestination(ResourceID rid) => Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");

    public override Stream GetResourceOutputStream(string buildDestination) {
        return new FileStream(buildDestination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
}
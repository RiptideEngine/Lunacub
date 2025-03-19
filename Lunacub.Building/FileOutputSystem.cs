﻿namespace Caxivitual.Lunacub.Building;

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
            if (!ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), out var rid)) continue;

            try {
                using FileStream reportFile = File.OpenRead(file);
                
                receiver.Add(rid, JsonSerializer.Deserialize<IncrementalInfo>(reportFile));
            } catch {
                // Ignore any failed attempt to deserialize report.
            }
        }
    }

    public override void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports) {
        foreach ((var rid, var report) in reports) {
            using FileStream reportFile = File.OpenWrite(Path.Combine(ReportDirectory, $"{rid}{CompilingConstants.ReportExtension}"));
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
        }
    }

    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        string path = Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.GetLastWriteTime(path) : null;
    }

    public override Stream CreateDestinationStream(ResourceID rid) {
        return new FileStream(Path.Combine(ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
}
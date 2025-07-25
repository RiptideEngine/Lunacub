﻿using System.Globalization;

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
        string searchPattern = $"*{CompilingConstants.ReportExtension}";
        
        foreach (var file in Directory.EnumerateFiles(ReportDirectory, searchPattern, SearchOption.TopDirectoryOnly)) {
            if (!ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, null, out var rid)) {
                continue;
            }

            try {
                using FileStream reportFile = File.OpenRead(file);
                
                receiver.Add(rid, JsonSerializer.Deserialize<IncrementalInfo>(reportFile));
            } catch (Exception) {
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
        string fileName = $"{rid:X}{CompilingConstants.CompiledResourceExtension}";
        string path = Path.Combine(ResourceOutputDirectory, fileName);
        
        return File.Exists(path) ? File.GetLastWriteTime(path) : null;
    }

    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        string fileName = $"{rid:X}{CompilingConstants.CompiledResourceExtension}";
        using FileStream fs = File.OpenWrite(Path.Combine(ResourceOutputDirectory, fileName));
        
        fs.SetLength(0);
        fs.Flush();
        
        sourceStream.CopyTo(fs);
    }

    public override void OutputResourceRegistry(ResourceRegistry<ResourceRegistry.Element> registry) {
        using FileStream fs = File.OpenWrite(Path.Combine(ResourceOutputDirectory, "__registry"));
        fs.SetLength(0);
        fs.Flush();

        JsonSerializer.Serialize(fs, registry.ToDictionary());
    }
}
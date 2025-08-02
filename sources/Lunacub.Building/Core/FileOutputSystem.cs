using Caxivitual.Lunacub.Building.Collections;
using System.Globalization;

namespace Caxivitual.Lunacub.Building.Core;

[ExcludeFromCodeCoverage]
public class FileOutputSystem : OutputSystem {
    public string IncrementalInfoDirectory { get; }
    public string ResourceOutputDirectory { get; }
    
    public FileOutputSystem(string incrementalInfoDirectory, string resourceOutputDirectory) {
        IncrementalInfoDirectory = Path.GetFullPath(incrementalInfoDirectory);
        if (!Directory.Exists(IncrementalInfoDirectory)) {
            throw new ArgumentException($"Incremental information directory '{IncrementalInfoDirectory}' does not exist.");
        }
        
        ResourceOutputDirectory = Path.GetFullPath(resourceOutputDirectory);
        if (!Directory.Exists(ResourceOutputDirectory)) {
            throw new ArgumentException($"Resource output directory '{ResourceOutputDirectory}' does not exist.");
        }
    }

    public override void CollectIncrementalInfos(EnvironmentIncrementalInfos receiver) {
        const string searchPattern = $"*{CompilingConstants.ReportExtension}";

        foreach (var libraryDirectoryPath in Directory.EnumerateDirectories(IncrementalInfoDirectory, "*", SearchOption.TopDirectoryOnly)) {
            ReadOnlySpan<char> libraryDirectoryName = Path.GetFileName(libraryDirectoryPath.AsSpan());

            if (!LibraryID.TryParse(libraryDirectoryName, null, out LibraryID libraryId)) continue;

            LibraryIncrementalInfos libraryIncrementalInfos = [];
            
            foreach (var reportFilePath in Directory.EnumerateFiles(libraryDirectoryPath, searchPattern, SearchOption.TopDirectoryOnly)) {
                ReadOnlySpan<char> idPart = Path.GetFileNameWithoutExtension(reportFilePath.AsSpan());
                
                if (!ResourceID.TryParse(idPart, NumberStyles.Integer, null, out ResourceID resourceId)) {
                    continue;
                }
                
                try {
                    using FileStream reportFile = File.OpenRead(reportFilePath);
    
                    libraryIncrementalInfos.Add(resourceId, JsonSerializer.Deserialize<IncrementalInfo>(reportFile));
                } catch (Exception) {
                    // Ignore any failed attempt to deserialize report.
                }
            }
            
            receiver.Add(libraryId, libraryIncrementalInfos);
        }
    }

    public override void FlushIncrementalInfos(EnvironmentIncrementalInfos incrementalInfos) {
        foreach ((var libraryId, var libraryIncrementalInfos) in incrementalInfos) {
            string libraryInfoPath = Path.Combine(IncrementalInfoDirectory, libraryId.ToString());

            if (Directory.Exists(libraryInfoPath)) {
                Directory.Delete(libraryInfoPath, recursive: true);
            }

            Directory.CreateDirectory(libraryInfoPath);

            foreach ((var resourceId, var incrementalInfo) in libraryIncrementalInfos) {
                string resourceInfoPath = Path.Combine(libraryInfoPath, $"{resourceId}{CompilingConstants.ReportExtension}");

                using FileStream stream = File.OpenWrite(resourceInfoPath);

                JsonSerializer.Serialize(stream, incrementalInfo);
            }
        }
    }

    public override DateTime? GetResourceLastBuildTime(ResourceAddress address) {
        string path = GetCompiledResourcePath(address);
        
        return File.Exists(path) ? File.GetLastWriteTime(path) : null;
    }

    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceAddress address) {
        string path = GetCompiledResourcePath(address);

        // Probably no need to if-checking but ehhh whatever i call it sanity checking.
        if (Path.GetDirectoryName(path) is { } directoryName) {
            Directory.CreateDirectory(directoryName);
        }

        using FileStream fs = File.OpenWrite(path);
        fs.SetLength(0);
        fs.Flush();
        
        sourceStream.CopyTo(fs);
    }

    public override void OutputLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId) {
        using FileStream fs = File.OpenWrite(GetLibraryRegistryPath(libraryId));
        fs.SetLength(0);
        fs.Flush();
        
        JsonSerializer.Serialize(fs, registry);
    }

    private string GetCompiledResourcePath(ResourceAddress address) {
        return Path.Combine(ResourceOutputDirectory, address.LibraryId.ToString(), $"{address.ResourceId}{CompilingConstants.CompiledResourceExtension}");
    }

    private string GetLibraryRegistryPath(LibraryID libraryId) {
        return Path.Combine(ResourceOutputDirectory, libraryId.ToString(), "__registry");
    }
}
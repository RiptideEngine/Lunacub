using Caxivitual.Lunacub.Building.Collections;
using System.Globalization;

namespace Caxivitual.Lunacub.Building.Core;

[ExcludeFromCodeCoverage]
public class FileOutputSystem : OutputSystem {
    public string BuildInformationDirectory { get; }
    public string ResourceOutputDirectory { get; }
    
    public FileOutputSystem(string buildInfoDirectory, string resourceOutputDirectory) {
        BuildInformationDirectory = Path.GetFullPath(buildInfoDirectory);
        if (!Directory.Exists(BuildInformationDirectory)) {
            throw new ArgumentException(string.Format(ExceptionMessages.BuildInfoDirectoryNotExist, BuildInformationDirectory));
        }
        
        ResourceOutputDirectory = Path.GetFullPath(resourceOutputDirectory);
        if (!Directory.Exists(ResourceOutputDirectory)) {
            throw new ArgumentException(string.Format(ExceptionMessages.ResourceOutputDirectoryNotExist, ResourceOutputDirectory));
        }
    }

    public override void CollectIncrementalInfos(EnvironmentIncrementalInfos receiver) {
        string filePath = IncrementalInfoFilePath;

        if (!File.Exists(filePath)) return;
        
        using var stream = File.OpenRead(filePath);

        try {
            if (JsonSerializer.Deserialize<EnvironmentIncrementalInfos>(stream) is not { } infos) return;
            
            foreach ((var libraryId, var libraryIncrementalInfo) in infos) {
                receiver.Add(libraryId, libraryIncrementalInfo);
            }
        } catch {
            // Ignored.
        }
    }

    public override void FlushIncrementalInfos(EnvironmentIncrementalInfos incrementalInfos) {
        using var stream = File.OpenWrite(IncrementalInfoFilePath);
        stream.SetLength(0);
        stream.Flush();
        
        JsonSerializer.Serialize(stream, incrementalInfos);
    }

    public override void CollectProceduralSchematic(EnvironmentProceduralSchematic receiver) {
        string filePath = ProceduralSchematicFilePath;

        if (!File.Exists(filePath)) return;
        
        using var stream = File.OpenRead(filePath);

        try {
            if (JsonSerializer.Deserialize<EnvironmentProceduralSchematic>(stream) is not { } infos) return;
            
            foreach ((var libraryId, var libraryIncrementalInfo) in infos) {
                receiver.Add(libraryId, libraryIncrementalInfo);
            }
        } catch {
            // Ignored.
        }
    }

    public override void FlushProceduralSchematic(EnvironmentProceduralSchematic schematic) {
        using var stream = File.OpenWrite(ProceduralSchematicFilePath);
        stream.SetLength(0);
        stream.Flush();
        
        JsonSerializer.Serialize(stream, schematic);
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
        return Path.Combine(ResourceOutputDirectory, address.LibraryId.ToString("X"), $"{address.ResourceId:X}{CompilingConstants.CompiledResourceExtension}");
    }

    private string GetLibraryRegistryPath(LibraryID libraryId) {
        return Path.Combine(ResourceOutputDirectory, libraryId.ToString("X"), "__registry");
    }

    private string IncrementalInfoFilePath => Path.Combine(BuildInformationDirectory, "incinfos.json");
    private string ProceduralSchematicFilePath => Path.Combine(BuildInformationDirectory, "procschema.json");
}
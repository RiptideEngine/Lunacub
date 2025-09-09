namespace Caxivitual.Lunacub.Building.Core;

public class FileBuildCacheIO : IBuildCacheIO {
    public string RootDirectory { get; }
    
    public FileBuildCacheIO(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException(string.Format(ExceptionMessages.DirectoryNotFound, RootDirectory));
        }
    }
    
    public void CollectIncrementalInfos(EnvironmentBuildCache receiver) {
        string filePath = IncrementalInfoFilePath;

        if (!File.Exists(filePath)) return;
        
        using var stream = File.OpenRead(filePath);

        try {
            if (JsonSerializer.Deserialize<EnvironmentBuildCache>(stream) is not { } infos) return;
            
            foreach ((var libraryId, var libraryIncrementalInfo) in infos) {
                receiver.Add(libraryId, libraryIncrementalInfo);
            }
        } catch {
            // Ignored.
        }
    }

    public void FlushBuildCaches(EnvironmentBuildCache buildCache) {
        using var stream = File.OpenWrite(IncrementalInfoFilePath);
        stream.SetLength(0);
        stream.Flush();
        
        JsonSerializer.Serialize(stream, buildCache);
    }
    
    private string IncrementalInfoFilePath => Path.Combine(RootDirectory, "incinfos.json");
}
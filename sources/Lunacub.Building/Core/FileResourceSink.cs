namespace Caxivitual.Lunacub.Building.Core;

[ExcludeFromCodeCoverage]
public class FileResourceSink : IResourceSink {
    public string RootDirectory { get; }
    
    public FileResourceSink(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException(string.Format(ExceptionMessages.DirectoryNotFound, RootDirectory));
        }
    }

    public void FlushCompiledResource(Stream sourceStream, ResourceAddress address) {
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

    public void FlushLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId) {
        using FileStream fs = File.OpenWrite(GetLibraryRegistryPath(libraryId));
        fs.SetLength(0);
        fs.Flush();
        
        JsonSerializer.Serialize(fs, registry);
    }

    private string GetCompiledResourcePath(ResourceAddress address) {
        return Path.Combine(RootDirectory, address.LibraryId.ToString("X"), $"{address.ResourceId:X}{CompilingConstants.CompiledResourceExtension}");
    }

    private string GetLibraryRegistryPath(LibraryID libraryId) {
        return Path.Combine(RootDirectory, libraryId.ToString("X"), "__registry");
    }
}
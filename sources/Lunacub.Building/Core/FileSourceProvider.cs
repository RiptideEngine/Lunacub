using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class FileSourceProvider : BuildSourceProvider {
    public string RootDirectory { get; set; }
    
    public FileSourceProvider(string rootDirectory) {
        RootDirectory = Path.GetFullPath(rootDirectory);
        
        if (!Directory.Exists(RootDirectory)) {
            throw new DirectoryNotFoundException($"Resource directory '{RootDirectory}' not found.");
        }
    }

    protected override Stream CreateStreamCore(string address) {
        string path = Path.Combine(RootDirectory, address);
        if (!path.StartsWith(RootDirectory)) {
            throw new InvalidResourceStreamException($"Concatenated resource path '{path}' lies outside root directory '{RootDirectory}'.");
        }
        
        return File.OpenRead(path);
    }

    public override DateTime GetLastWriteTime(string address) {
        string path = Path.Combine(RootDirectory, address);
        if (!path.StartsWith(RootDirectory)) {
            throw new InvalidResourceStreamException($"Concatenated resource path '{path}' lies outside root directory '{RootDirectory}'.");
        }

        return File.GetLastWriteTime(path);
    }

    // protected override Stream? CreateResourceStreamCore(ResourceID resourceId, BuildingResource element) {
    //     string path = Path.Combine(RootDirectory, element.Address);
    //     if (!path.StartsWith(RootDirectory)) return null;
    //
    //     return File.OpenRead(path);
    // }
    //
    // protected override DateTime GetResourceLastWriteTimeCore(ResourceID resourceId, BuildingResource resource) {
    //     string path = Path.Combine(RootDirectory, resource.Address);
    //     if (!path.StartsWith(RootDirectory)) return default;
    //     
    //     return File.GetLastWriteTime(path);
    // }
}
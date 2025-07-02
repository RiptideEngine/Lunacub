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
            string message = string.Format(ExceptionMessages.SourceAddressOutsideRootDirectory, path, RootDirectory);
            throw new InvalidResourceStreamException(message);
        }
        
        return File.OpenRead(path);
    }

    public override DateTime GetLastWriteTime(string address) {
        string path = Path.Combine(RootDirectory, address);
        if (!path.StartsWith(RootDirectory)) {
            string message = string.Format(ExceptionMessages.SourceAddressOutsideRootDirectory, path, RootDirectory);
            throw new InvalidResourceStreamException(message);
        }

        return File.GetLastWriteTime(path);
    }
}
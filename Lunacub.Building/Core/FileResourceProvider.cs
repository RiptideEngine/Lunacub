namespace Caxivitual.Lunacub.Building.Core;

public sealed class FileResourceProvider : ResourceProvider {
    private readonly string _path;
    
    public override DateTime LastWriteTime => File.GetLastWriteTime(_path);
    
    public FileResourceProvider(string path) {
        _path = Path.GetFullPath(path);
    }
    
    public override Stream GetStream() => File.OpenRead(_path);
}
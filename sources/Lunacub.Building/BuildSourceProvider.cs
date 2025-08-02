namespace Caxivitual.Lunacub.Building;

public abstract class BuildSourceProvider : SourceProvider<string> {
    public abstract DateTime GetLastWriteTime(string address);
}
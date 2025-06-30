namespace Caxivitual.Lunacub.Building;

public abstract class BuildSourceProvider : SourceProvider {
    public abstract DateTime GetLastWriteTime(string address);
}
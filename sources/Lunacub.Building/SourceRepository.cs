namespace Caxivitual.Lunacub.Building;

public abstract class SourceRepository : SourceRepository<string> {
    public abstract DateTime GetLastWriteTime(string address);
}
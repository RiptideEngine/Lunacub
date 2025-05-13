namespace Caxivitual.Lunacub.Building;

public abstract class ResourceProvider {
    public abstract DateTime GetLastWriteTime();
    public abstract Stream GetStream();
}
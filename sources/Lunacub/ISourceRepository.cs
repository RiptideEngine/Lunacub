namespace Caxivitual.Lunacub;

public interface ISourceRepository<in TAddress> {
    Stream? CreateStream(TAddress address);
}
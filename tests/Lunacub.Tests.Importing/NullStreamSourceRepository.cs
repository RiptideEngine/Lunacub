using SourceRepository = Caxivitual.Lunacub.Importing.SourceRepository;

namespace Caxivitual.Lunacub.Tests.Importing;

public sealed class NullStreamSourceRepository : SourceRepository {
    protected override Stream? CreateStreamCore(ResourceID resourceId) => null;
}
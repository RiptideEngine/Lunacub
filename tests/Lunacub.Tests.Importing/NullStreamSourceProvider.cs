namespace Caxivitual.Lunacub.Tests.Importing;

public sealed class NullStreamSourceProvider : ImportSourceProvider {
    protected override Stream? CreateStreamCore(ResourceID resourceId) => null;
}
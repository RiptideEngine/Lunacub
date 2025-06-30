using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Importing;

public abstract class ImportSourceProvider : SourceProvider {
    public Stream? CreateStream(ResourceID resourceId) {
        if (CreateStreamCore(resourceId) is { } stream) {
            if (!stream.CanRead || !stream.CanSeek) {
                throw new InvalidResourceStreamException($"Returned Stream for resource with Id {resourceId} must be readable and seekable.");
            }
    
            if (stream.CanWrite) {
                throw new InvalidResourceStreamException($"Returned Stream for resource with Id {resourceId} must not be writable for security reason.");
            }

            return stream;
        }

        return null;
    }

    protected abstract Stream? CreateStreamCore(ResourceID resourceId);
}
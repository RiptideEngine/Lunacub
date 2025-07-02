using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Importing;

public abstract class ImportSourceProvider : SourceProvider {
    public Stream? CreateStream(ResourceID resourceId) {
        if (CreateStreamCore(resourceId) is { } stream) {
            if (!stream.CanRead || !stream.CanSeek) {
                string message = string.Format(ExceptionMessages.ResourceStreamMustBeSeekableOrReadable, resourceId);
                throw new InvalidResourceStreamException(message);
            }
    
            if (stream.CanWrite) {
                string message = string.Format(ExceptionMessages.ResourceStreamMustNotBeWritable, resourceId);
                throw new InvalidResourceStreamException(message);
            }

            return stream;
        }

        return null;
    }

    protected abstract Stream? CreateStreamCore(ResourceID resourceId);
}
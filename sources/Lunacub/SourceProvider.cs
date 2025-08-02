using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub;

public abstract class SourceProvider<T> {
    public Stream? CreateStream(T address) {
        if (CreateStreamCore(address) is { } stream) {
            if (!stream.CanRead || !stream.CanSeek) {
                string message = string.Format(ExceptionMessages.SourceStreamMustBeSeekableOrReadable, address);
                throw new InvalidResourceStreamException(message);
            }
    
            if (stream.CanWrite) {
                string message = string.Format(ExceptionMessages.SourceStreamMustNotBeWritable, address);
                throw new InvalidResourceStreamException(message);
            }

            return stream;
        }

        return null;
    }

    protected abstract Stream? CreateStreamCore(T address);
}
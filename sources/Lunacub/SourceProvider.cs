using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub;

public abstract class SourceProvider {
    public Stream? CreateStream(string address) {
        if (CreateStreamCore(address) is { } stream) {
            if (!stream.CanRead || !stream.CanSeek) {
                throw new InvalidResourceStreamException($"Returned Stream at address '{address}' must be readable and seekable.");
            }
    
            if (stream.CanWrite) {
                throw new InvalidResourceStreamException($"Returned Stream at address '{address}' must not be writable for security reason.");
            }

            return stream;
        }

        return null;
    }

    protected abstract Stream? CreateStreamCore(string address);
}
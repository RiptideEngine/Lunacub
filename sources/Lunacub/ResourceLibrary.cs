using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub;

public abstract class ResourceLibrary<TElementOption> {
    public ResourceRegistry<TElementOption> Registry { get; }

    protected ResourceLibrary(ResourceRegistry<TElementOption> registry) {
        Registry = registry;
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry<TElementOption>.Element element)) return null;

        Stream? stream = CreateResourceStreamCore(resourceId, element.Option);
        
        ValidateStream(stream);

        return stream;
    }

    protected abstract Stream? CreateResourceStreamCore(ResourceID resourceId, TElementOption options);

    private static void ValidateStream(Stream? stream) {
        if (stream != null) {
            if (!stream.CanRead || !stream.CanSeek) {
                throw new InvalidResourceStreamException("Returned Stream must be readable and seekable.");
            }
    
            if (stream.CanWrite) {
                throw new InvalidResourceStreamException("Returned Stream must not be writable for security reason.");
            }
        }
    }
}
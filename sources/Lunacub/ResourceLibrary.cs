namespace Caxivitual.Lunacub;

public abstract class ResourceLibrary<TElement> where TElement : IRegistryElement {
    public ResourceRegistry<TElement> Registry { get; }

    protected ResourceLibrary(ResourceRegistry<TElement> registry) {
        Registry = registry;
    }

    public Stream? CreateResourceStream(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out TElement? element)) return null;

        Stream? stream = CreateResourceStreamCore(resourceId, element);
        
        ValidateStream(stream);

        return stream;
    }

    protected abstract Stream? CreateResourceStreamCore(ResourceID resourceId, TElement element);

    private static void ValidateStream(Stream? stream) {
        if (stream != null) {
            if (!stream.CanRead || !stream.CanSeek) {
                throw new InvalidOperationException("Created Stream must be readable and seekable.");
            }
    
            if (stream.CanWrite) {
                throw new InvalidOperationException("Created Stream must not be writable for security reason.");
            }
        }
    }
}
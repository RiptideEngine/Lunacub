using System.Collections;
using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

public abstract class ResourceLibrary : IEnumerable<ResourceID> {
    public Guid Id { get; }
    
    protected ResourceLibrary(Guid id) {
        Id = id;
    }

    public abstract bool Contains(ResourceID rid);

    public Stream? CreateStream(ResourceID rid) {
        Stream? created = CreateStreamImpl(rid);
        
        if (created != null && (!created.CanRead || !created.CanSeek)) {
            throw new InvalidOperationException("Created Stream must be readable and seekable.");
        }

        return created;
    }
    protected abstract Stream? CreateStreamImpl(ResourceID rid);
    
    public abstract IEnumerator<ResourceID> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
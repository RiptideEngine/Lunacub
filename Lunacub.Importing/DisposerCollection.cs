using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
public sealed class DisposerCollection : Collection<Disposer> {
    internal DisposerCollection() {}
    
    protected override void InsertItem(int index, Disposer item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, Disposer item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.SetItem(index, item);
    }
    
    internal bool TryDispose(object resource) {
        foreach (var disposer in this) {
            if (disposer.TryDispose(resource)) return true;
        }

        return false;
    }
}
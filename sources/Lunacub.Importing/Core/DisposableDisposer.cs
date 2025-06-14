namespace Caxivitual.Lunacub.Importing.Core;

public sealed class DisposableDisposer : Disposer {
    public override bool TryDispose(object resource) {
        if (resource is not IDisposable disposable) return false;
        
        disposable.Dispose();
        return true;
    }
}
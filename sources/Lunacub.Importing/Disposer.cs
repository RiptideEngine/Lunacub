namespace Caxivitual.Lunacub.Importing;

public abstract class Disposer {
    public abstract bool TryDispose(object resource);

    public static Disposer Create(Func<object, bool> disposeFunc) => new DelegateDisposer(disposeFunc);
}

file sealed class DelegateDisposer(Func<object, bool> disposeFunc) : Disposer {
    public override bool TryDispose(object resource) => disposeFunc(resource);
}
namespace Caxivitual.Lunacub.Importing;

public readonly struct ImportingOperation {
    public readonly ResourceID Rid;
    public readonly Task<ResourceHandle> Task;

    internal ImportingOperation(ResourceID rid, Task<ResourceHandle> task) {
        Rid = rid;
        Task = task;
    }
}

public readonly struct ImportingOperation<T> where T : class {
    public readonly ResourceID Rid;
    public readonly Task<ResourceHandle<T>> Task;

    internal ImportingOperation(ResourceID rid, Task<ResourceHandle<T>> task) {
        Rid = rid;
        Task = task;
    }
}
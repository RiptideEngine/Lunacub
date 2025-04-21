namespace Caxivitual.Lunacub.Importing;

public struct ImportingOperation {
    public readonly ResourceID Rid;
    public readonly Task<ResourceHandle> Task;

    internal ImportingOperation(ResourceID rid, Task<ResourceHandle> task) {
        Rid = rid;
        Task = task;
    }
}
namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents a weakly-typed handle of the resource importing operation.
/// </summary>
public readonly struct ImportingOperation {
    /// <summary>
    /// The Id of the importing resource.
    /// </summary>
    public readonly ResourceID Rid;
    
    /// <summary>
    /// The importing task.
    /// </summary>
    public readonly Task<ResourceHandle> Task;

    internal ImportingOperation(ResourceID rid, Task<ResourceHandle> task) {
        Rid = rid;
        Task = task;
    }

    /// <summary>
    /// Gets an awaiter used to await the underlying importing task.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    /// <see cref="Task"/>
    public TaskAwaiter<ResourceHandle> GetAwaiter() => Task.GetAwaiter();
}

/// <summary>
/// Represents a strongly-typed handle of the resource importing operation.
/// </summary>
/// <typeparam name="T">Resource type to import. Must be a reference type.</typeparam>
public readonly struct ImportingOperation<T> where T : class {
    /// <inheritdoc cref="ImportingOperation.Rid"/>
    public readonly ResourceID Rid;
    
    /// <inheritdoc cref="ImportingOperation.Task"/>
    public readonly Task<ResourceHandle<T>> Task;

    internal ImportingOperation(ResourceID rid, Task<ResourceHandle<T>> task) {
        Rid = rid;
        Task = task;
    }

    /// <summary>
    /// Gets an awaiter used to await the underlying importing task.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    /// <see cref="Task"/>
    public TaskAwaiter<ResourceHandle<T>> GetAwaiter() => Task.GetAwaiter();
}
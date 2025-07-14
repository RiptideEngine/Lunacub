namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents a weakly-typed handle of the resource importing operation.
/// </summary>
public readonly struct ImportingOperation {
    private readonly ResourceCache.ElementContainer _resourceContainer;

    /// <summary>
    /// The Id of the importing resource.
    /// </summary>
    public ResourceID ResourceId => _resourceContainer.ResourceId;

    /// <summary>
    /// The name of the importing resource.
    /// </summary>
    public string ResourceName => _resourceContainer.ResourceName;
    
    /// <summary>
    /// The importing task.
    /// </summary>
    public Task<ResourceHandle> Task => _resourceContainer.FinalizeTask;
    
    public ImportingStatus Status => _resourceContainer.Status;
    
    internal ImportingOperation(ResourceCache.ElementContainer resourceContainer) {
        _resourceContainer = resourceContainer;
    }

    /// <summary>
    /// Gets an awaiter used to await the underlying importing task.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    /// <see cref="Task"/>
    public TaskAwaiter<ResourceHandle> GetAwaiter() => Task.GetAwaiter();
}

// /// <summary>
// /// Represents a strongly-typed handle of the resource importing operation.
// /// </summary>
// /// <typeparam name="T">Resource type to import. Must be a reference type.</typeparam>
// public readonly struct ImportingOperation<T> where T : class {
//     private readonly ResourceCache.ElementContainer _resourceContainer;
//
//     /// <inheritdoc cref="ImportingOperation.Rid"/>
//     public ResourceID Rid => _resourceContainer.ResourceId;
//
//     /// <inheritdoc cref="ImportingOperation.Task"/>
//     public Task<ResourceHandle<T>> Task => _resourceContainer.FinalizeTask;
//
//     internal ImportingOperation(ResourceCache.ElementContainer resourceContainer resourceContainer) {
//         _resourceContainer = resourceContainer;
//     }
//
//     /// <summary>
//     /// Gets an awaiter used to await the underlying importing task.
//     /// </summary>
//     /// <returns>An awaiter instance.</returns>
//     /// <see cref="Task"/>
//     public TaskAwaiter<ResourceHandle<T>> GetAwaiter() => Task.GetAwaiter();
// }
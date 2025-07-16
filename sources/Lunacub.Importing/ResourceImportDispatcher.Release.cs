namespace Caxivitual.Lunacub.Importing;

partial class ResourceImportDispatcher {
    public ReleaseStatus Release(ResourceID resourceId) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (resourceId == ResourceID.Null) return ReleaseStatus.Null;
        if (_cache.Get(resourceId) is not { } container) return ReleaseStatus.NotImported;

        return ReleaseContainer(container);
    }

    public ReleaseStatus Release(object? resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (resource == null!) return ReleaseStatus.Null;
        if (_cache.Get(resource) is not { } container) return ReleaseStatus.InvalidResource;
        
        container.EnsureCancellationTokenSourceIsDisposed();
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.Status == ImportingStatus.Success);
        
        Debug.Assert(ReferenceEquals(container.FinalizeTask.Result.Value, resource));
        
        _environment.Statistics.ReleaseReferences();

        if (container.DecrementReference() != 0) return ReleaseStatus.Success;
        
        _environment.Statistics.DecrementUniqueResourceCount();
        
        bool removedSuccessfully = _cache.RemoveResourceMap(resource);
        Debug.Assert(removedSuccessfully);

        removedSuccessfully = _cache.Remove(container.ResourceId);
        Debug.Assert(removedSuccessfully);

        ReleaseReferences(container.ReferenceResourceIds);
        
        container.Status = ImportingStatus.Disposed;
        return DisposeResource(resource) ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
    }

    public ReleaseStatus Release(ResourceHandle handle) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        (ResourceID resourceId, object? resource) = handle;
        
        if (resourceId == ResourceID.Null || resource == null) return ReleaseStatus.Null;
        if (_cache.Get(resource) is not { } container) return ReleaseStatus.InvalidResource;
        
        container.EnsureCancellationTokenSourceIsDisposed();
        
        if (container.ResourceId != resourceId) return ReleaseStatus.IdIncompatible;
        if (container.Status != ImportingStatus.Success) return ReleaseStatus.InvalidResource;
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.FinalizeTask.Result == handle);
        
        _environment.Statistics.ReleaseReferences();

        if (container.DecrementReference() != 0) return ReleaseStatus.Success;
        
        _environment.Statistics.DecrementUniqueResourceCount();
        
        bool removedSuccessfully = _cache.RemoveResourceMap(resource);
        Debug.Assert(removedSuccessfully);

        removedSuccessfully = _cache.Remove(resourceId);
        Debug.Assert(removedSuccessfully);

        ReleaseReferences(container.ReferenceResourceIds);
        
        container.Status = ImportingStatus.Disposed;
        return DisposeResource(resource) ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
    }

    public ReleaseStatus Release(ImportingOperation operation) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (operation.ResourceId == ResourceID.Null || operation.UnderlyingContainer == null) return ReleaseStatus.Null;
        if (_cache.Get(operation.ResourceId) is not { } container) return ReleaseStatus.InvalidOperationId;
        if (!ReferenceEquals(container, operation.UnderlyingContainer)) return ReleaseStatus.InvalidOperationContainer;

        return ReleaseContainer(container);
    }

    private ReleaseStatus ReleaseContainer(ResourceCache.ElementContainer container) {
        Debug.Assert(container.ReferenceCount != 0);
        
        _environment.Statistics.ReleaseReferences();
        if (container.DecrementReference() != 0) return ReleaseStatus.Success;
        
        container.CancellationTokenSource?.Cancel();

        try {
            container.FinalizeTask.Wait();
        } catch (AggregateException ae) {
            foreach (var e in ae.InnerExceptions) {
                if (e is TaskCanceledException or OperationCanceledException) continue;

                // TODO: Report
            }
        }

        switch (container.FinalizeTask.Status) {
            case TaskStatus.RanToCompletion:
                Debug.Assert(container.Status == ImportingStatus.Success);
                container.EnsureCancellationTokenSourceIsDisposed();
                
                _environment.Statistics.DecrementUniqueResourceCount();

                ResourceHandle handle = container.FinalizeTask.Result;
                Debug.Assert(handle.ResourceId == container.ResourceId);

                bool removedSuccessfully = _cache.RemoveResourceMap(handle.Value!);
                Debug.Assert(removedSuccessfully);

                removedSuccessfully = _cache.Remove(handle.ResourceId);
                Debug.Assert(removedSuccessfully);

                ReleaseReferences(container.ReferenceResourceIds);

                container.Status = ImportingStatus.Disposed;
                return DisposeResource(handle.Value!) ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
            
            case TaskStatus.Canceled or TaskStatus.Faulted:
                container.EnsureCancellationTokenSourceIsDisposed();
                Debug.Assert(container.Status is ImportingStatus.Canceled or ImportingStatus.Failed);
                Debug.Assert(_cache.Contains(container.ResourceId));
                return ReleaseStatus.Canceled;
            
            default:
                string message = string.Format(ExceptionMessages.UnexpectedTaskStatus, container.FinalizeTask.Status, container.ResourceId);
                throw new UnreachableException(message);
        }
    }

    private void ReleaseReferences(IReadOnlySet<ResourceID> resourceIds) {
        foreach (var reference in resourceIds) {
            Release(reference);
        }
    }
}
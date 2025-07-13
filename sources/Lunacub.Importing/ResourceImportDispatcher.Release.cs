namespace Caxivitual.Lunacub.Importing;

partial class ResourceImportDispatcher {
    public ReleaseStatus Release(ResourceID resourceId) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.Get(resourceId) is not { } container) return ReleaseStatus.NotImported;
        
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
        
        Debug.Assert(container.CancellationTokenSource == null);

        switch (container.FinalizeTask.Status) {
            case TaskStatus.RanToCompletion:
                Debug.Assert(container.Status == ImportingStatus.Success);
                
                _environment.Statistics.DecrementUniqueResourceCount();

                ResourceHandle handle = container.FinalizeTask.Result;
                Debug.Assert(handle.ResourceId == resourceId);

                bool removedSuccessfully = _cache.RemoveResourceMap(handle.Value!);
                Debug.Assert(removedSuccessfully);

                removedSuccessfully = _cache.Remove(resourceId);
                Debug.Assert(removedSuccessfully);

                ReleaseReferences(container.ReferenceResourceIds);

                container.Status = ImportingStatus.Disposed;
                return DisposeResource(handle.Value!) ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
            
            case TaskStatus.Canceled or TaskStatus.Faulted:
                Debug.Assert(container.Status == ImportingStatus.Failed);
                Debug.Assert(_cache.Contains(resourceId));
                return ReleaseStatus.Canceled;
            
            default:
                string message = string.Format(ExceptionMessages.UnexpectedTaskStatus, container.FinalizeTask.Status, resourceId);
                throw new UnreachableException(message);
        }
    }

    public ReleaseStatus Release(object resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (resource == null!) return ReleaseStatus.Null;
        
        if (_cache.Get(resource) is not { } container) return ReleaseStatus.InvalidResource;
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.Status == ImportingStatus.Success);
        Debug.Assert(container.CancellationTokenSource == null);
        
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
        if (container.ResourceId != resourceId) return ReleaseStatus.IdIncompatible;
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.Status == ImportingStatus.Success);
        Debug.Assert(container.CancellationTokenSource == null);
        
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

    private void ReleaseReferences(IReadOnlySet<ResourceID> resourceIds) {
        foreach (var reference in resourceIds) {
            Release(reference);
        }
    }
}
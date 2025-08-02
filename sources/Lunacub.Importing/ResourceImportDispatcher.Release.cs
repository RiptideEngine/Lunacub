using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

partial class ResourceImportDispatcher {
    public ReleaseStatus Release(ResourceAddress resourceAddress) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (resourceAddress.IsNull) return ReleaseStatus.Null;
        if (Cache.Get(resourceAddress) is not { } container) return ReleaseStatus.NotImported;

        return ReleaseContainer(container);
    }

    public ReleaseStatus Release(object? resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (resource == null!) return ReleaseStatus.Null;
        if (Cache.Get(resource) is not { } container) return ReleaseStatus.InvalidResource;
        
        container.EnsureCancellationTokenSourceIsDisposed();
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.Status == ImportingStatus.Success);
        
        Debug.Assert(ReferenceEquals(container.FinalizeTask.Result.Value, resource));
        
        _environment.Statistics.ReleaseReferences();

        if (container.DecrementReference() != 0) return ReleaseStatus.Success;
        
        _environment.Statistics.DecrementUniqueResourceCount();
        
        bool removedSuccessfully = Cache.RemoveResourceMap(resource);
        Debug.Assert(removedSuccessfully);

        removedSuccessfully = Cache.Remove(container.Address);
        Debug.Assert(removedSuccessfully);

        ReleaseReferences(container.ReferenceResourceAddresses);
        
        container.Status = ImportingStatus.Disposed;
        return DisposeResource(resource) ? ReleaseStatus.Disposed : ReleaseStatus.NotDisposed;
    }

    public ReleaseStatus Release(ResourceHandle handle) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        (ResourceAddress resourceAddress, object? resource) = handle;
        
        if (resourceAddress.IsNull || resource == null) return ReleaseStatus.Null;
        if (Cache.Get(resource) is not { } container) return ReleaseStatus.InvalidResource;
        
        container.EnsureCancellationTokenSourceIsDisposed();
        
        if (container.Address != resourceAddress) return ReleaseStatus.IdIncompatible;
        if (container.Status != ImportingStatus.Success) return ReleaseStatus.InvalidResource;
        
        Debug.Assert(container.FinalizeTask.Status == TaskStatus.RanToCompletion);
        Debug.Assert(container.FinalizeTask.Result == handle);
        
        _environment.Statistics.ReleaseReferences();

        if (container.DecrementReference() != 0) return ReleaseStatus.Success;
        
        _environment.Statistics.DecrementUniqueResourceCount();
        
        bool removedSuccessfully = Cache.RemoveResourceMap(resource);
        Debug.Assert(removedSuccessfully);

        removedSuccessfully = Cache.Remove(resourceAddress);
        Debug.Assert(removedSuccessfully);

        ReleaseReferences(container.ReferenceResourceAddresses);
        
        container.Status = ImportingStatus.Disposed;
        return DisposeResource(resource) ? ReleaseStatus.Disposed : ReleaseStatus.NotDisposed;
    }

    public ReleaseStatus Release(ImportingOperation operation) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (operation.Address.IsNull || operation.UnderlyingContainer == null) return ReleaseStatus.Null;
        if (Cache.Get(operation.Address) is not { } container) return ReleaseStatus.InvalidOperationId;
        if (!ReferenceEquals(container, operation.UnderlyingContainer)) return ReleaseStatus.InvalidOperationContainer;

        return ReleaseContainer(container);
    }

    private ReleaseStatus ReleaseContainer(ResourceCache.ElementContainer container) {
        Debug.Assert(container.ReferenceCount != 0);
        
        _environment.Statistics.ReleaseReferences();
        if (container.DecrementReference() != 0) return ReleaseStatus.Success;

        using (container.EnterLockScope()) {
            container.CancelImport();
        }

        ResourceHandle handle;

        try {
            handle = container.FinalizeTask.ConfigureAwait(false).GetAwaiter().GetResult();
        } catch (Exception e) when (e is TaskCanceledException or OperationCanceledException) {
            container.EnsureCancellationTokenSourceIsNotDisposed();
            Debug.Assert(container.Status is ImportingStatus.Canceled);
            
            return ReleaseStatus.Canceled;
        } catch (Exception) {
            container.EnsureCancellationTokenSourceIsDisposed();
            Debug.Assert(container.Status is ImportingStatus.Failed);
            
            return ReleaseStatus.Success;
        }
        
        container.EnsureCancellationTokenSourceIsDisposed();
        
        Debug.Assert(container.Status == ImportingStatus.Success);
        Debug.Assert(handle.Address == container.Address);

        _environment.Statistics.DecrementUniqueResourceCount();

        bool removedSuccessfully = Cache.RemoveResourceMap(handle.Value!);
        Debug.Assert(removedSuccessfully);

        removedSuccessfully = Cache.Remove(handle.Address);
        Debug.Assert(removedSuccessfully);

        ReleaseReferences(container.ReferenceResourceAddresses);

        container.Status = ImportingStatus.Disposed;
        return DisposeResource(handle.Value!) ? ReleaseStatus.Success : ReleaseStatus.NotDisposed;
    }

    private void ReleaseReferences(IReadOnlySet<ResourceAddress> resourceIds) {
        foreach (var reference in resourceIds) {
            Release(reference);
        }
    }
}
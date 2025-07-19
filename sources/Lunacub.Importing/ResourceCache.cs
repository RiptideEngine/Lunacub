using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Runtime.ExceptionServices;

namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents the thread-safe containers to store resource container object, and provides access from resource object to
/// <see cref="ResourceID"/>.
/// </summary>
internal sealed class ResourceCache : IDisposable, IAsyncDisposable {
    private bool _disposed;

    // private readonly SemaphoreSlim _lock;
    private readonly ImportEnvironment _environment;
    private readonly Lock _lock;
    
    private readonly Dictionary<ResourceID, ElementContainer> _containers;
    private readonly Dictionary<object, ResourceID> _resourceMap;
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ResourceCache(ImportEnvironment environment) {
        _environment = environment;
        _lock = new();
        _containers = [];
        _resourceMap = [];
    }

    public ElementContainer? Get(ResourceID resourceId) {
        using (_lock.EnterScope()) {
            return _containers.GetValueOrDefault(resourceId);
        }
    }

    public ElementContainer? Get(object resource) {
        using (_lock.EnterScope()) {
            return _resourceMap.TryGetValue(resource, out var id) ? _containers[id] : null;
        }
    }

    public bool Remove(ResourceID resourceId) {
        using (_lock.EnterScope()) {
            return _containers.Remove(resourceId);
        }
    }

    public bool Contains(ResourceID resourceId) {
        using (_lock.EnterScope()) {
            return _containers.ContainsKey(resourceId);
        }
    }

    public ElementContainer GetOrBeginImporting(
        ResourceID resourceId,
        Action<ElementContainer> action,
        Func<ResourceID, ElementContainer> factory
    ) {
        using (_lock.EnterScope()) {
            ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_containers, resourceId, out bool exists);

            if (!exists) {
                reference = factory(resourceId);

                if (reference == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }
            } else {
                action(reference!);
            }

            return reference!;
        }
    }
    
    public ElementContainer GetOrBeginImporting<TArg>(
        ResourceID resourceId,
        Action<ElementContainer, TArg> action,
        Func<ResourceID, TArg, ElementContainer> factory,
        TArg arg
    ) where TArg : allows ref struct {
        using (_lock.EnterScope()) {
            ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_containers, resourceId, out bool exists);
            
            if (!exists) {
                reference = factory(resourceId, arg);
                
                if (reference == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }
            } else {
                action(reference!, arg);
            }
            
            return reference!;
        }
    }

    public void RegisterResourceMap(object resource, ResourceID resourceId) {
        using (_lock.EnterScope()) {
            _resourceMap.Add(resource, resourceId);
        }
    }

    public bool RemoveResourceMap(object resource) {
        using (_lock.EnterScope()) {
            return _resourceMap.Remove(resource);
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            using (_lock.EnterScope()) {
                Task.WaitAll(_containers.Values.Select(async container => {
                    using (container.EnterLockScope()) {
                        container.CancelImport();
                    }
                    
                    ResourceHandle handle;

                    try {
                        handle = await container.FinalizeTask;
                    } catch {
                        // Ignored.
                        return;
                    }
                    
                    container.EnsureCancellationTokenSourceIsDisposed();

                    if (_environment.Disposers.TryDispose(handle.Value!)) {
                        _environment.Statistics.IncrementDisposedResourceCount();
                    } else {
                        _environment.Statistics.IncrementUndisposedResourceCount();
                    }

                    container.ReferenceCount = 0;
                    container.Status = ImportingStatus.Disposed;
                }));

                _containers.Clear();
                _environment.Statistics.ResetReferenceCounts();
                _environment.Statistics.ResetUniqueResourceCount();
            }
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        _lock.Enter();

        try {
            await Task.WhenAll(_containers.Values.Select(async container => {
                // await container.CancellationTokenSource?.CancelAsync() ?? Task.CompletedTask;
                
                
                ResourceHandle handle;

                try {
                    handle = await container.FinalizeTask;
                } catch {
                    // Ignored.
                    return;
                }

                if (_environment.Disposers.TryDispose(handle.Value!)) {
                    _environment.Statistics.IncrementDisposedResourceCount();
                } else {
                    _environment.Statistics.IncrementUndisposedResourceCount();
                }

                container.ReferenceCount = 0;
                container.Status = ImportingStatus.Disposed;
            })).ConfigureAwait(false);

            _containers.Clear();
            _environment.Statistics.ResetReferenceCounts();
            _environment.Statistics.ResetUniqueResourceCount();
        } finally {
            _lock.Exit();
        }
    }

    ~ResourceCache() {
        Dispose(false);
    }

    /// <summary>
    /// Represents an outer shell resource container, containing reference count, reference ids, status, etc...
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public sealed class ElementContainer {
        public readonly ResourceID ResourceId;
        public readonly string ResourceName;

        public uint ReferenceCount;
        
        public FrozenSet<ResourceID> ReferenceResourceIds;

        public ImportingStatus Status;

        private CancellationTokenSource? _cancellationTokenSource;
        public CancellationToken CancellationToken => _cancellationTokenSource!.Token;
        
        public Task<ResourceImportDispatcher.ResourceImportResult>? ImportTask { get; set; }
        public Task<ResourceImportDispatcher.ReferenceResolveResult>? ResolvingReferenceTask { get; set; }
        public Task<ResourceHandle> FinalizeTask { get; set; }

        private readonly Lock _lock;

        public ElementContainer(ResourceID resourceId, string resourceName) {
            ResourceId = resourceId;
            ResourceName = resourceName;
            ReferenceResourceIds = FrozenSet<ResourceID>.Empty;
            Status = ImportingStatus.Importing;
            FinalizeTask = null!;
            ReferenceCount = 1;
            _cancellationTokenSource = null!;
            _lock = new();
        }

        public void InitializeImport() {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (_cancellationTokenSource == null) {
                _cancellationTokenSource = new();
            } else {
                switch (Status) {
                    case ImportingStatus.Disposed or ImportingStatus.Canceled:
                        _cancellationTokenSource = new();
                        break;
                    
                    case ImportingStatus.Importing or ImportingStatus.Success:
                        throw new InvalidOperationException();
                    
                    case ImportingStatus.Failed:
                        // No longer belong to cache.
                        throw new UnreachableException();
                }
            }

            Status = ImportingStatus.Importing;
        }

        public uint IncrementReference() {
            return Interlocked.Increment(ref ReferenceCount);
        }
        
        public uint DecrementReference() {
            uint initialValue, computedValue;
            do {
                initialValue = ReferenceCount;
                computedValue = ReferenceCount == 0 ? 0 : ReferenceCount - 1;
            } while (initialValue != Interlocked.CompareExchange(ref ReferenceCount, computedValue, initialValue));
        
            return computedValue;
        }

        public uint ResetReferenceCounter() {
            return Interlocked.Exchange(ref ReferenceCount, 0);
        }
        
        public void CancelImport() {
            _cancellationTokenSource?.Cancel();
        }

        public void DisposeCancellationToken() {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        [Conditional("DEBUG")]
        public void EnsureCancellationTokenSourceIsNotDisposed() {
            Debug.Assert(_cancellationTokenSource != null);
        }

        [Conditional("DEBUG")]
        public void EnsureCancellationTokenSourceIsDisposed() {
            Debug.Assert(_cancellationTokenSource == null);
        }

        public void EnterLock() {
            _lock.Enter();
        }

        public void ExitLock() {
            _lock.Exit();
        }

        public Lock.Scope EnterLockScope() {
            return _lock.EnterScope();
        }
    }
}
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
    
    private readonly Dictionary<ResourceAddress, ElementContainer> _containers;
    private readonly Dictionary<object, ResourceAddress> _resourceMap;
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ResourceCache(ImportEnvironment environment) {
        _environment = environment;
        _lock = new();
        _containers = [];
        _resourceMap = [];
    }

    public ElementContainer? Get(ResourceAddress address) {
        using (_lock.EnterScope()) {
            return _containers.GetValueOrDefault(address);
        }
    }

    public ElementContainer? Get(object resource) {
        using (_lock.EnterScope()) {
            return _resourceMap.TryGetValue(resource, out var id) ? _containers[id] : null;
        }
    }

    public bool Remove(ResourceAddress address) {
        using (_lock.EnterScope()) {
            return _containers.Remove(address);
        }
    }

    public bool Contains(ResourceAddress address) {
        using (_lock.EnterScope()) {
            return _containers.ContainsKey(address);
        }
    }

    public ElementContainer GetOrBeginImporting(
        ResourceAddress address,
        Action<ElementContainer> action,
        Func<ResourceAddress, ElementContainer> factory
    ) {
        using (_lock.EnterScope()) {
            ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_containers, address, out bool exists);

            if (!exists) {
                reference = factory(address);

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
        ResourceAddress address,
        Action<ElementContainer, TArg> action,
        Func<ResourceAddress, TArg, ElementContainer> factory,
        TArg arg
    ) where TArg : allows ref struct {
        using (_lock.EnterScope()) {
            ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(_containers, address, out bool exists);
            
            if (!exists) {
                reference = factory(address, arg);
                
                if (reference == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }
            } else {
                action(reference!, arg);
            }
            
            return reference!;
        }
    }

    public void RegisterResourceMap(object resource, ResourceAddress address) {
        using (_lock.EnterScope()) {
            _resourceMap.Add(resource, address);
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
        public readonly ResourceAddress Address;
        public readonly string ResourceName;

        public uint ReferenceCount;
        
        public FrozenSet<ResourceAddress> ReferenceResourceAddresses;

        public ImportingStatus Status;

        internal CancellationTokenSource? CancellationTokenSource { get; private set; }
        public CancellationToken CancellationToken => CancellationTokenSource!.Token;
        
        public Task<ResourceImportDispatcher.ResourceImportResult>? ImportTask { get; set; }
        public Task<ResourceHandle> FinalizeTask { get; set; }

        private readonly Lock _lock;

        public ElementContainer(ResourceAddress address, string resourceName) {
            Address = address;
            ResourceName = resourceName;
            ReferenceResourceAddresses = FrozenSet<ResourceAddress>.Empty;
            Status = ImportingStatus.Importing;
            FinalizeTask = null!;
            ReferenceCount = 1;
            CancellationTokenSource = null!;
            _lock = new();
        }

        public void InitializeImport() {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (CancellationTokenSource == null) {
                CancellationTokenSource = new();
            } else {
                switch (Status) {
                    case ImportingStatus.Disposed or ImportingStatus.Canceled:
                        CancellationTokenSource = new();
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
            CancellationTokenSource?.Cancel();
        }

        public void DisposeCancellationToken() {
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;
        }

        public void EnsureCancellationTokenSourceIsNotDisposed() {
            Debug.Assert(CancellationTokenSource != null);
        }

        public void EnsureCancellationTokenSourceIsDisposed() {
            Debug.Assert(CancellationTokenSource == null);
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
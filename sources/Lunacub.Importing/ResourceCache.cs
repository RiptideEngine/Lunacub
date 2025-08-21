using Caxivitual.Lunacub.Collections;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Represents the thread-safe containers to store resource container object, and provides access from resource object to
/// <see cref="ResourceID"/>.
/// </summary>
internal sealed class ResourceCache : IDisposable {
    private bool _disposed;

    // private readonly SemaphoreSlim _lock;
    private readonly ImportEnvironment _environment;
    private readonly Lock _lock;
    
    private readonly SortedList<LibraryID, LibraryResourceCache> _libraryCaches;
    private readonly Dictionary<object, ResourceAddress> _resourceMap;
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ResourceCache(ImportEnvironment environment) {
        _environment = environment;
        _lock = new();
        _libraryCaches = [];
        _resourceMap = [];
    }

    public ElementContainer? Get(ResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            return _libraryCaches.TryGetValue(address.LibraryId, out var libraryCache) ? libraryCache.Containers.GetValueOrDefault(address.ResourceId) : null;
        }
    }

    public ElementContainer? Get(LibraryID libraryId, ReadOnlySpan<char> name) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            return _libraryCaches.TryGetValue(libraryId, out var libraryCache) ? 
                libraryCache.NameMap.TryGetValue(name, out var resourceId) ? 
                    libraryCache.Containers[resourceId] : 
                    null : 
                null;
        }
    }

    public ElementContainer? Get(object resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            if (!_resourceMap.TryGetValue(resource, out var address)) return null;

            return _libraryCaches[address.LibraryId].Containers[address.ResourceId];
        }
    }

    public bool Remove(ResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            if (_libraryCaches.TryGetValue(address.LibraryId, out var libraryCache)) {
                if (libraryCache.Containers.Remove(address.ResourceId, out var removed)) {
                    if (removed.ResourceName != null) {
                        libraryCache.NameMap.Remove(removed.ResourceName);
                    }

                    return true;
                }
            }

            return false;
        }
    }

    public bool Remove(LibraryID libraryId, ReadOnlySpan<char> name) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            if (_libraryCaches.TryGetValue(libraryId, out var libraryCache)) {
                if (libraryCache.NameMap.Remove(name, out _, out ResourceID removedId)) {
                    bool removedSuccessfully = libraryCache.Containers.Remove(removedId);
                    Debug.Assert(removedSuccessfully);
                    
                    return true;
                }
            }

            return false;
        }
    }

    public bool Contains(ResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            return _libraryCaches.TryGetValue(address.LibraryId, out var libraryCache) && libraryCache.Containers.ContainsKey(address.ResourceId);
        }
    }

    public bool Contains(LibraryID libraryId, ReadOnlySpan<char> name) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            return _libraryCaches.TryGetValue(libraryId, out var libraryCache) && libraryCache.NameMap.ContainsKey(name);
        }
    }
    
    public ElementContainer GetOrBeginImporting(
        ResourceAddress address,
        Action<ElementContainer> action,
        ElementFactory<ResourceAddress> factory
    ) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            if (!_libraryCaches.TryGetValue(address.LibraryId, out var libraryCache)) {
                libraryCache = new([], new(StringComparer.Ordinal));
                _libraryCaches.Add(address.LibraryId, libraryCache);
            }

            if (libraryCache.Containers.TryGetValue(address.ResourceId, out ElementContainer? resourceContainer)) {
                action(resourceContainer);
            } else {
                bool add = true;
                resourceContainer = factory(address, ref add);
            
                if (resourceContainer == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }

                if (add) {
                    libraryCache.Containers.Add(address.ResourceId, resourceContainer);

                    if (resourceContainer.ResourceName is { } name) {
                        libraryCache.NameMap.Add(name, address.ResourceId);
                    }
                }
            }

            return resourceContainer;
        }
    }

    public ElementContainer GetOrBeginImporting<TArg>(
        ResourceAddress address,
        Action<ElementContainer, TArg> action,
        ElementFactory<ResourceAddress, TArg> factory,
        TArg arg
    ) where TArg : allows ref struct {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            if (!_libraryCaches.TryGetValue(address.LibraryId, out var libraryCache)) {
                libraryCache = new([], new(StringComparer.Ordinal));
                _libraryCaches.Add(address.LibraryId, libraryCache);
            }

            if (libraryCache.Containers.TryGetValue(address.ResourceId, out ElementContainer? resourceContainer)) {
                action(resourceContainer, arg);
            } else {
                bool add = true;
                resourceContainer = factory(address, arg, ref add);
            
                if (resourceContainer == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }

                if (add) {
                    libraryCache.Containers.Add(address.ResourceId, resourceContainer);

                    if (resourceContainer.ResourceName is { } name) {
                        libraryCache.NameMap.Add(name, address.ResourceId);
                    }
                }
            }

            return resourceContainer;
        }
    }
    
    public ElementContainer GetOrBeginImporting(
        SpanNamedResourceAddress address,
        Action<ElementContainer> action,
        ElementFactory<SpanNamedResourceAddress> factory
    ) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            (LibraryID libraryId, ReadOnlySpan<char> name) = address;
            
            if (!_libraryCaches.TryGetValue(libraryId, out var libraryCache)) {
                libraryCache = new([], new(StringComparer.Ordinal));
                _libraryCaches.Add(libraryId, libraryCache);
            }

            ElementContainer container;

            if (libraryCache.NameMap.TryGetValue(name, out var resourceId)) {
                action(container = libraryCache.Containers[resourceId]);
            } else {
                bool add = true;
                container = factory(address, ref add);
            
                if (container == null) {
                    throw new InvalidOperationException("Factory must return non-null instance.");
                }

                if (add) {
                    resourceId = container.Address.ResourceId;
                    
                    if (!name.Equals(container.ResourceName, StringComparison.Ordinal)) {
                        throw new InvalidOperationException("Created container must have the same resource name as argument.");
                    }
                    
                    libraryCache.Containers.Add(resourceId, container);
                    libraryCache.NameMap.Add(name.ToString(), resourceId);
                }
            }

            return container;
        }
    }

    public void RegisterResourceMap(object resource, ResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            _resourceMap.Add(resource, address);
        }
    }

    public bool RemoveResourceMap(object resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using (_lock.EnterScope()) {
            return _resourceMap.Remove(resource);
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            _environment.Logger.LogDebug("ResourceCache: Disposing...");
            
            using (_lock.EnterScope()) {
                foreach ((_, var libraryCache) in _libraryCaches) {
                    foreach ((_, var container) in libraryCache.Containers) {
                        using (container.EnterLockScope()) {
                            container.CancelImport();
                        }
                    }
                }
                
                foreach ((_, var libraryCache) in _libraryCaches) {
                    Task.WaitAll(((IDictionary<ResourceID, ElementContainer>)libraryCache.Containers).Values.Select(async container => {
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
                    
                        container.ResetReferenceCounter();
                        container.Status = ImportingStatus.Disposed;
                    }));
                }
            
                _libraryCaches.Clear();
                _environment.Statistics.ResetReferenceCounts();
                _environment.Statistics.ResetUniqueResourceCount();
            }
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
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
        public readonly string? ResourceName;

        public uint ReferenceCount;
        
        public FrozenSet<ResourceAddress> ReferenceResourceAddresses;

        public ImportingStatus Status;

        internal CancellationTokenSource? CancellationTokenSource { get; private set; }
        public CancellationToken CancellationToken => CancellationTokenSource!.Token;
        
        public Task<ResourceImportDispatcher.ResourceImportResult>? ImportTask { get; set; }
        public Task<ResourceHandle> FinalizeTask { get; set; }

        private readonly Lock _lock;

        public ElementContainer(ResourceAddress address, string? resourceName) {
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

    public delegate ElementContainer ElementFactory<in TAddress>(TAddress address, ref bool add) where TAddress : allows ref struct;
    public delegate ElementContainer ElementFactory<in TAddress, in TArg>(TAddress address, TArg arg, ref bool add) where TAddress : allows ref struct where TArg : allows ref struct;

    private readonly record struct LibraryResourceCache(ResourceIdentityDictionary<ElementContainer> Containers, IdentityDictionary<ResourceID> NameMap);
}
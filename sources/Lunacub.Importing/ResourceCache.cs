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

    public ValueTask DisposeAsync() {
        if (Interlocked.Exchange(ref _disposed, true)) return ValueTask.CompletedTask;

        // TODO: Implementation
        return ValueTask.CompletedTask;
    }

    ~ResourceCache() {
        Dispose(false);
    }

    /// <summary>
    /// Represents an outer shell resource container, containing reference count, reference ids, status, etc...
    /// </summary>
    public sealed class ElementContainer {
        public readonly ResourceID ResourceId;
        public readonly string ResourceName;

        public uint ReferenceCount;
        
        public FrozenSet<ResourceID> ReferenceResourceIds;

        public ImportingStatus Status;
        
        public CancellationTokenSource? CancellationTokenSource { get; private set; }
        public CancellationToken CancellationToken => CancellationTokenSource!.Token;
        
        public Task<ResourceImportDispatcher.ResourceImportResult>? ImportTask { get; set; }
        public Task<ResourceImportDispatcher.ReferenceResolveResult>? ResolvingReferenceTask { get; set; }
        public Task<ResourceHandle> FinalizeTask { get; set; }

        public ElementContainer(ResourceID resourceId, string resourceName) {
            ResourceId = resourceId;
            ResourceName = resourceName;
            ReferenceResourceIds = FrozenSet<ResourceID>.Empty;
            Status = ImportingStatus.Importing;
            FinalizeTask = null!;
            ReferenceCount = 1;
            CancellationTokenSource = null!;
        }

        public void InitializeImport() {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (CancellationTokenSource == null) {
                CancellationTokenSource = new();
            } else {
                switch (Status) {
                    case ImportingStatus.Canceled:
                        bool resetSuccessfully = CancellationTokenSource.TryReset();
                        Debug.Assert(resetSuccessfully);
                        break;
                    
                    case ImportingStatus.Disposed:
                        CancellationTokenSource = new();
                        break;
                    
                    case ImportingStatus.Importing or ImportingStatus.Success:
                        throw new InvalidOperationException();
                    
                    case ImportingStatus.Failed:
                        // Too busy to think about this case. Will handle later.
                        throw new NotImplementedException();
                }
            }
        }

        public uint IncrementReference() {
            uint initialValue, computedValue;
            do {
                initialValue = ReferenceCount;
                computedValue = ReferenceCount == 0 ? 0 : ReferenceCount + 1;
            } while (initialValue != Interlocked.CompareExchange(ref ReferenceCount, computedValue, initialValue));

            return computedValue;
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

        public void NullifyCancellationTokenSource() {
            CancellationTokenSource = null;
        }

        [Conditional("DEBUG")]
        public void EnsureCancellationTokenSourceIsDisposed() {
            Debug.Assert(CancellationTokenSource != null! && IsDisposed(CancellationTokenSource));

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_disposed")]
            static extern ref bool IsDisposed(CancellationTokenSource cts);
        }
    }
}
// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Compilation;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

internal sealed partial class ResourceImportDispatcher : IDisposable {
    private readonly ImportEnvironment _environment;

    internal ResourceCache Cache { get; }
    
    private bool _disposed;

    public ResourceImportDispatcher(ImportEnvironment environment) {
        _environment = environment;
        Cache = new(environment);
    }

    public ImportingOperation Import(ResourceID resourceId) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!_environment.Libraries.ContainsResource(resourceId, out ResourceRegistry.Element element)) {
            string message = string.Format(ExceptionMessages.UnregisteredResourceId, resourceId);

            ResourceCache.ElementContainer container = new(resourceId, string.Empty) {
                FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(resourceId))),
                ReferenceCount = 0,
                Status = ImportingStatus.Failed,
            };

            return new(container);
        }
        
        return new(Cache.GetOrBeginImporting(resourceId, ProcessCachedContainer, BeginImport, element));
        
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId, ResourceRegistry.Element element) {
            ResourceCache.ElementContainer container = new(resourceId, element.Name);
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, container.ResourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            return container;
        }
    }

    public ImportingOperation Import(ReadOnlySpan<char> name) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_environment.Libraries.ContainsResource(name, out ResourceID id)) {
            string message = string.Format(ExceptionMessages.UnregisteredResourceName, name.ToString());

            ResourceCache.ElementContainer container = new(ResourceID.Null, name.ToString()) {
                FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(name))),
                ReferenceCount = 0,
                Status = ImportingStatus.Failed,
            };

            return new(container);
        }

        return new(Cache.GetOrBeginImporting(id, ProcessCachedContainer, BeginImport, name));
        
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId, ReadOnlySpan<char> name) {
            ResourceCache.ElementContainer container = new(resourceId, name.ToString());
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, container.ResourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            return container;
        }
    }
    
    private async Task<ResourceHandle> ImportingTask(ResourceCache.ElementContainer container) {
        container.EnsureCancellationTokenSourceIsNotDisposed();
        Debug.Assert(!container.CancellationToken.IsCancellationRequested);
        
        ResourceHandle handle;
        IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers;
        IReadOnlyCollection<ResourceID> releaseReferences;

        try {
            (handle, waitContainers, releaseReferences) = await ResolveReference(container);
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Canceled);
            
            // Not remove from the cache to allow reimport later in the future.
            throw;
        } catch {
            Debug.Assert(container.Status == ImportingStatus.Failed);
            
            // Remove from the cache.
            bool removedSuccessfully = Cache.Remove(container.ResourceId);
            Debug.Assert(removedSuccessfully);
            
            throw;
        }

        try {
            // Sanity check.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            Debug.Assert(waitContainers.All(x => x.FinalizeTask != null), "Unexpected null FinalizeTask.");

            await Task.WhenAll(waitContainers.Select(async x => {
                try {
                    await x.FinalizeTask;
                } catch (OperationCanceledException) {
                } catch (Exception e) {
                    // TODO: Log?
                }
            }));

            _environment.Statistics.IncrementUniqueResourceCount();

            Cache.RegisterResourceMap(handle.Value!, container.ResourceId);
            container.Status = ImportingStatus.Success;

            foreach (var releasingId in releaseReferences) {
                Release(releasingId);
            }

            return handle;
        } catch (Exception e) {
            Debug.Assert(e is not OperationCanceledException, "Should not happen.");
            Debug.Assert(container.Status == ImportingStatus.Failed);

            Log.FinalizeTaskExceptionOccured(_environment.Logger, container.ResourceId);

            DisposeResource(handle.Value!);

            bool removedSuccessfully = Cache.Remove(container.ResourceId);
            Debug.Assert(removedSuccessfully);

            throw;
        } finally {
            using (container.EnterLockScope()) {
                container.DisposeCancellationToken();
                container.EnsureCancellationTokenSourceIsDisposed();
            }
        }
    }
    
    private async Task<ReferenceResolveResult> ResolveReference(ResourceCache.ElementContainer container) {
        Deserializer deserializer;
        object resource;
        DeserializationContext context;
        
        try {
            (deserializer, resource, context) = await (container.ImportTask = ImportVesselTask(container));
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Canceled);
            Debug.Assert(container.ReferenceCount == 0);

            throw;
        } catch (Exception e) when (e is not OperationCanceledException) {
            Log.ResolveReferenceExceptionOccured(_environment.Logger, container.ResourceId);
            
            Debug.Assert(container.Status == ImportingStatus.Failed);
            Debug.Assert(container.ReferenceCount == 0);
            
            throw;
        }

        try {
            ResourceHandle handle = new(container.ResourceId, resource);
            
            if (context.RequestingReferences.Count == 0) {
                return new(handle, ReadOnlyCollection<ResourceCache.ElementContainer>.Empty, ReadOnlyCollection<ResourceID>.Empty);
            }
            
            container.CancellationToken.ThrowIfCancellationRequested();

            Log.BeginResolvingReference(_environment.Logger, container.ResourceId);

            IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers = 
                await ProcessReferences(container, context, resource, deserializer);

            Log.EndResolvingReference(_environment.Logger, container.ResourceId);

            List<ResourceID> releaseReferences = [];
            foreach (var releasingReference in context.RequestingReferences.ReleasedReferences) {
                bool getSuccessfully = context.RequestingReferences.References!.TryGetValue(releasingReference, out var referenceHandle);
                Debug.Assert(getSuccessfully);
                
                releaseReferences.Add(referenceHandle.ResourceId);
            }
            
            return new(handle, waitContainers, releaseReferences);
        } catch (OperationCanceledException) {
            ReleaseContainerReferenceCounter(container);
            DisposeResource(resource);
            
            throw;
        }
    }
    
    private async Task<ResourceImportResult> ImportVesselTask(ResourceCache.ElementContainer container) {
        await Task.Yield();

        try {
            if (_environment.Libraries.CreateResourceStream(container.ResourceId) is not { } stream) {
                string message = string.Format(ExceptionMessages.NullResourceStream, container.ResourceId);
                throw new InvalidOperationException(message);
            }

            BinaryHeader header;

            try {
                header = BinaryHeader.Extract(stream);
            } catch {
                await stream.DisposeAsync();
                throw;
            }

            switch (header.MajorVersion) {
                case 1:
                    return await ResourceImporterVersion1.ImportVessel(_environment, stream, header, container.CancellationToken);

                default:
                    string message = string.Format(
                        ExceptionMessages.UnsupportedCompiledResourceVersion,
                        header.MajorVersion,
                        header.MinorVersion
                    );
                    throw new ArgumentException(message);
            }
        } catch (OperationCanceledException) {
            Log.CancelImport(_environment.Logger, container.ResourceId);

            using (container.EnterLockScope()) {
                ReleaseContainerReferenceCounter(container);
                container.Status = ImportingStatus.Canceled;
            }

            throw;
        } catch (Exception e) {
            Log.ReportImportException(_environment.Logger, container.ResourceId, e);
            
            ReleaseContainerReferenceCounter(container);

            using (container.EnterLockScope()) {
                container.DisposeCancellationToken();
                container.Status = ImportingStatus.Failed;
            }

            throw;
        }
    }

    private async Task<IReadOnlyCollection<ResourceCache.ElementContainer>> ProcessReferences(ResourceCache.ElementContainer container, DeserializationContext context, object resource, Deserializer deserializer) {
        context.RequestingReferences.DisableRequest();
        
        var collectedResult = await CollectReferences(container, resource, context.RequestingReferences.Requesting);
        
        container.ReferenceResourceIds = collectedResult.References.Select(x => x.Value.Container.ResourceId).ToFrozenSet();

        // Resolving references.
        try {
            context.RequestingReferences.References = collectedResult.References.ToDictionary(x => x.Key, x => x.Value.Handle);
            deserializer.ResolveReferences(resource, context);
        } catch {
            // Ignored.
        }

        return collectedResult.WaitContainers;
    }

    private async Task<ReferenceCollectingResult> CollectReferences(
        ResourceCache.ElementContainer currentContainer,
        object currentResource,
        IReadOnlyDictionary<ReferencePropertyKey, RequestingReferences.RequestingReference> requestReferences
    ) {
        Dictionary<ReferencePropertyKey, ReferenceImportResult> references = new(requestReferences.Count);
        List<ResourceCache.ElementContainer> waitContainers = [];
        
        // RecursivelyCollectReferences(container, requestReferences);

        foreach ((var referencePropertyKey, var requesting) in requestReferences) {
            requesting.Deconstruct(out ResourceID requestingId);

            if (requestingId == currentContainer.ResourceId) {
                currentContainer.IncrementReference();
                _environment.Statistics.AddReference();
                
                references.Add(referencePropertyKey, new(new(requesting.ResourceId, currentResource), currentContainer));
                continue;
            }
            
            if (!_environment.Libraries.ContainsResource(requesting.ResourceId, out ResourceRegistry.Element element)) continue;
            
            ResourceCache.ElementContainer referenceContainer =
                Cache.GetOrBeginImporting(requesting.ResourceId, ProcessCachedContainer, BeginReferenceImport, element.Name);
            
            try {
                (_, object vessel, _) = await referenceContainer.ImportTask!;
    
                references.Add(referencePropertyKey, new(new(requesting.ResourceId, vessel), referenceContainer));
            } catch {
                Debug.Assert(referenceContainer.Status == ImportingStatus.Failed);
                Debug.Assert(referenceContainer.ReferenceCount == 0);
            }
        }
        
        return new(references, waitContainers);
        
        ResourceCache.ElementContainer BeginReferenceImport(ResourceID resourceId, string resourceName) {
            Debug.Assert(_environment.Libraries.ContainsResource(resourceId));
            
            Log.BeginImport(_environment.Logger, resourceId);
            
            ResourceCache.ElementContainer container = new(resourceId, resourceName);
            _environment.Statistics.AddReference();
            
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            waitContainers.Add(container);
            
            return container;
        }
    }
    
    private void ProcessCachedContainer<T>(ResourceCache.ElementContainer container, T unusedArg) where T : allows ref struct {
        switch (container.Status) {
            case ImportingStatus.Success or ImportingStatus.Importing:
                uint incremented = container.IncrementReference();
                Debug.Assert(incremented != 0);
                
                _environment.Statistics.AddReference();
                break;
            
            case ImportingStatus.Canceled or ImportingStatus.Disposed:
                using (container.EnterLockScope()) {
                    Debug.Assert(container.ReferenceCount == 0);

                    container.IncrementReference();

                    _environment.Statistics.AddReference();

                    Log.BeginImport(_environment.Logger, container.ResourceId);

                    container.InitializeImport();
                    container.FinalizeTask = ImportingTask(container);
                }

                break;
            
            case ImportingStatus.Failed: throw new UnreachableException();
        }
    }

    private void ReleaseContainerReferenceCounter(ResourceCache.ElementContainer container) {
        _environment.Statistics.ReleaseReferences(container.ResetReferenceCounter());
    }

    private bool DisposeResource(object resource) {
        if (_environment.Disposers.TryDispose(resource)) {
            _environment.Statistics.IncrementDisposedResourceCount();
            return true;
        }

        _environment.Statistics.IncrementUndisposedResourceCount();
        return false;
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (disposing) {
            Cache.Dispose();
        }
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceImportDispatcher() {
        Dispose(false);
    }
    
    public readonly record struct ResourceImportResult(
        Deserializer Deserializer, 
        object Resource, 
        DeserializationContext Context
    );
    public readonly record struct ReferenceResolveResult(
        ResourceHandle Handle, 
        IReadOnlyCollection<ResourceCache.ElementContainer> WaitContainers,
        IReadOnlyCollection<ResourceID> ReleaseReferences
    );

    private readonly record struct ReferenceImportResult(
        ResourceHandle Handle, 
        ResourceCache.ElementContainer Container
    );
    private readonly record struct ReferenceCollectingResult(
        Dictionary<ReferencePropertyKey, ReferenceImportResult> References,
        IReadOnlyCollection<ResourceCache.ElementContainer> WaitContainers
    );

    private sealed class ReferenceContainerEqualityComparer : EqualityComparer<ResourceCache.ElementContainer> {
        public static ReferenceContainerEqualityComparer Instance { get; } = new();
        
        public override bool Equals(ResourceCache.ElementContainer? x, ResourceCache.ElementContainer? y) {
            if (ReferenceEquals(x, y)) return true;
            if ((x != null && y == null) || (x == null && y != null)) return false;
            
            Debug.Assert(x != null && y != null);
            
            return x.ResourceId.Equals(y.ResourceId);
        }
        
        public override int GetHashCode(ResourceCache.ElementContainer container) => container.ResourceId.GetHashCode();
    }
}
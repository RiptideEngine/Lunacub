// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Compilation;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

internal sealed partial class ResourceImportDispatcher : IDisposable {
    private readonly ImportEnvironment _environment;
    private readonly ResourceCache _cache;
    private bool _disposed;

    public ResourceImportDispatcher(ImportEnvironment environment) {
        _environment = environment;
        _cache = new(environment);
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
        
        return new(_cache.GetOrBeginImporting(resourceId, IncrementContainerReference, BeginImport, element));
        
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId, ResourceRegistry.Element element) {
            ResourceCache.ElementContainer container = new(resourceId, element.Name);
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, container.ResourceId);
        
            container.InitializeImport();
            // container.ImportTask = ImportVesselTask(container);
            // container.ResolvingReferenceTask = ResolveReference(container);
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

        return new(_cache.GetOrBeginImporting(id, IncrementContainerReference, BeginImport, name));
        
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId, ReadOnlySpan<char> name) {
            ResourceCache.ElementContainer container = new(resourceId, name.ToString());
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, container.ResourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            return container;
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

    private async Task<ReferenceResolveResult> ResolveReference(
        ResourceCache.ElementContainer container
    ) {
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
            if (context.RequestingReferences.Count == 0) {
                return new(new(container.ResourceId, resource), ReadOnlyCollection<ResourceCache.ElementContainer>.Empty);
            }
            
            container.CancellationToken.ThrowIfCancellationRequested();

            context.RequestingReferences.DisableRequest();

            Log.BeginResolvingReference(_environment.Logger, container.ResourceId);

            // Retrieving reference resources.
            (var references, var waitContainers) =
                await CollectReferences(resource, container, context.RequestingReferences.Requesting);

            container.ReferenceResourceIds = references.Select(x => x.Value.Container.ResourceId).ToFrozenSet();

            // Resolving references.
            try {
                context.RequestingReferences.SetReferences(references.ToDictionary(x => x.Key, x => x.Value.Handle));
                deserializer.ResolveReferences(resource, context);
            } catch {
                // Ignored.
            }

            Log.EndResolvingReference(_environment.Logger, container.ResourceId);
            return new(new(container.ResourceId, resource), waitContainers);
        } catch (OperationCanceledException) {
            ReleaseContainerReferenceCounter(container);
            DisposeResource(resource);
            
            throw;
        }
    }

    // Collect the references and the waiting containers.
    private async Task<ReferenceCollectResult> CollectReferences(
        object currentResource,
        ResourceCache.ElementContainer currentContainer,
        IReadOnlyDictionary<ReferencePropertyKey, RequestingReferences.RequestingReference> requestingReferences
    ) {
        Dictionary<ReferencePropertyKey, ReferenceImportResult> references = [];
        HashSet<ResourceCache.ElementContainer> waitContainers = new(ReferenceContainerEqualityComparer.Instance);

        // Depth first traversing the importing graph, collect the reference containers.
        foreach ((var propertyKey, var requesting) in requestingReferences) {
            // TODO: Make it parallel and support cancellation token.
            
            if (requesting.ResourceId == currentContainer.ResourceId) {
                currentContainer.IncrementReference();
                _environment.Statistics.AddReference();
                
                references.Add(propertyKey, new(new(requesting.ResourceId, currentResource), currentContainer));

                continue;
            }

            if (!_environment.Libraries.ContainsResource(requesting.ResourceId, out ResourceRegistry.Element element)) continue;

            // TODO: Fix bug: we somehow got to
            
            ResourceCache.ElementContainer referenceContainer =
                _cache.GetOrBeginImporting(requesting.ResourceId, IncrementContainerReference, BeginReferenceImport, element.Name);

            if (referenceContainer.Status == ImportingStatus.Failed) continue;
            
            try {
                (_, object vessel, _) = await referenceContainer.ImportTask!;
                
                references.Add(propertyKey, new(new(requesting.ResourceId, vessel), referenceContainer));
            } catch {
                Debug.Assert(referenceContainer.Status == ImportingStatus.Failed);
                Debug.Assert(referenceContainer.ReferenceCount == 0);
            }
        }

        return new(references, waitContainers);
        
        ResourceCache.ElementContainer BeginReferenceImport(ResourceID resourceId, string resourceName) {
            Debug.Assert(_environment.Libraries.ContainsResource(resourceId));
            
            ResourceCache.ElementContainer container = new(resourceId, resourceName);
            _environment.Statistics.AddReference();
            
            container.InitializeImport();
            container.ImportTask = ImportVesselTask(container);
            container.ResolvingReferenceTask = ResolveReference(container);
            container.FinalizeTask = ImportingTask(container);

            waitContainers.Add(container);
            
            return container;
        }
    }
    
    private async Task<ResourceHandle> ImportingTask(ResourceCache.ElementContainer container) {
        ResourceHandle handle;
        IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers;

        try {
            (handle, waitContainers) = await (container.ResolvingReferenceTask = ResolveReference(container));
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Canceled);
            
            // Not remove from the cache to allow reimport later in the future.
            throw;
        } catch {
            Debug.Assert(container.Status == ImportingStatus.Failed);
            
            // Remove from the cache.
            bool removedSuccessfully = _cache.Remove(container.ResourceId);
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

            _cache.RegisterResourceMap(handle.Value!, container.ResourceId);
            container.Status = ImportingStatus.Success;

            return handle;
        } catch (Exception e) {
            Debug.Assert(e is not OperationCanceledException, "Should not happen.");
            Debug.Assert(container.Status == ImportingStatus.Failed);

            Log.FinalizeTaskExceptionOccured(_environment.Logger, container.ResourceId);

            DisposeResource(handle.Value!);

            bool removedSuccessfully = _cache.Remove(container.ResourceId);
            Debug.Assert(removedSuccessfully);

            throw;
        } finally {
            using (container.EnterLockScope()) {
                container.DisposeCancellationToken();
                container.EnsureCancellationTokenSourceIsDisposed();
            }
        }
    }
    
    private void IncrementContainerReference<T>(ResourceCache.ElementContainer container, T unusedArg) where T : allows ref struct {
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
                    container.ImportTask = ImportVesselTask(container);
                    container.ResolvingReferenceTask = ResolveReference(container);
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
            _cache.Dispose();
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
        IReadOnlyCollection<ResourceCache.ElementContainer> WaitContainers
    );

    private readonly record struct ReferenceImportResult(
        ResourceHandle Handle, 
        ResourceCache.ElementContainer Container
    );
    private readonly record struct ReferenceCollectResult(
        Dictionary<ReferencePropertyKey, ReferenceImportResult> References,
        IReadOnlySet<ResourceCache.ElementContainer> WaitContainers
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
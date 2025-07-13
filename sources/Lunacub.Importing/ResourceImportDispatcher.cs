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
        
        if (!_environment.Libraries.ContainsResource(resourceId)) {
            string message = string.Format(ExceptionMessages.UnregisteredResource, resourceId);

            ResourceCache.ElementContainer container = new(resourceId) {
                FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(resourceId))),
                ReferenceCount = 0,
                Status = ImportingStatus.Failed,
            };

            return new(container);
        }
        
        return new(_cache.GetOrBeginImporting(resourceId, IncrementContainerReference, BeginImport));
        
        // ReSharper disable once VariableHidesOuterVariable
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId) {
            ResourceCache.ElementContainer container = new(resourceId);
            
            _environment.Statistics.AddReference();
            
            Log.BeginImport(_environment.Logger, container.ResourceId);
            
            container.InitializeImport();
            container.ImportTask = ImportTask(container);
            container.ResolvingReferenceTask = ResolveReference(container);
            container.FinalizeTask = FinalizeTask(container);
            
            return container;
        }
    }
    
    private async Task<ResourceImportResult> ImportTask(ResourceCache.ElementContainer container) {
        await Task.Yield();

        try {
            if (_environment.Libraries.CreateResourceStream(container.ResourceId) is not { } stream) {
                // _environment.Logger.LogDebug("Missing Resource Stream.");

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

            ReleaseContainerReferenceCounter(container);

            container.Status = ImportingStatus.Cancelled;
            
            Debug.Assert(container.CancellationTokenSource.IsCancellationRequested);
            container.CancellationTokenSource.Dispose();
            
            throw;
        } catch (Exception e) {
            _environment.Logger.LogError("ImportTask: Exception occured. Stacktrace: {ex}", e);
            
            ReleaseContainerReferenceCounter(container);

            container.Status = ImportingStatus.Failed;
            container.CancellationTokenSource.Dispose();
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
            (deserializer, resource, context) = await container.ImportTask!;
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Cancelled);
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

            if (!_environment.Libraries.ContainsResource(requesting.ResourceId)) continue;

            ResourceCache.ElementContainer referenceContainer =
                _cache.GetOrBeginImporting(requesting.ResourceId, IncrementContainerReference, BeginReferenceImport);

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
        
        ResourceCache.ElementContainer BeginReferenceImport(ResourceID resourceId) {
            Debug.Assert(_environment.Libraries.ContainsResource(resourceId));
            
            ResourceCache.ElementContainer container = new(resourceId);
            _environment.Statistics.AddReference();
            
            container.InitializeImport();
            container.ImportTask = ImportTask(container);
            container.ResolvingReferenceTask = ResolveReference(container);
            container.FinalizeTask = FinalizeTask(container);

            waitContainers.Add(container);
            
            return container;
        }
    }
    
    private async Task<ResourceHandle> FinalizeTask(ResourceCache.ElementContainer container) {
        ResourceHandle handle;
        IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers;

        try {
            (handle, waitContainers) = await container.ResolvingReferenceTask!;
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Failed);
            Debug.Assert(container.CancellationTokenSource.IsCancellationRequested);
            
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
            container.CancellationTokenSource.Dispose();
        }
    }
    
    private void IncrementContainerReference(ResourceCache.ElementContainer container) {
        switch (container.Status) {
            case ImportingStatus.Success or ImportingStatus.Importing:
                uint incremented = container.IncrementReference();
                Debug.Assert(incremented != 0);

                _environment.Statistics.AddReference();
                break;
            
            case ImportingStatus.Cancelled or ImportingStatus.Disposed:
                Debug.Assert(container.ReferenceCount == 0);

                container.ReferenceCount = 1;
                _environment.Statistics.AddReference();
            
                Log.BeginImport(_environment.Logger, container.ResourceId);
            
                container.InitializeImport();
                container.ImportTask = ImportTask(container);
                container.ResolvingReferenceTask = ResolveReference(container);
                container.FinalizeTask = FinalizeTask(container);
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
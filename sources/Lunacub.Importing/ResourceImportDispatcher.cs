using Caxivitual.Lunacub.Compilation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceImportDispatcher : IDisposable {
    private readonly ImportEnvironment _environment;
    private readonly ResourceCache _cache;
    private bool _disposed;

    public ResourceImportDispatcher(ImportEnvironment environment) {
        _environment = environment;
        _cache = new();
    }

    public ImportingOperation Import(ResourceID resourceId) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return new(_cache.GetOrBeginImporting(resourceId, IncrementContainerReference, BeginImport));
        
        // ReSharper disable once VariableHidesOuterVariable
        ResourceCache.ElementContainer BeginImport(ResourceID resourceId) {
            ResourceCache.ElementContainer container = new(resourceId);

            if (!_environment.Libraries.ContainsResource(resourceId)) {
                string message = string.Format(ExceptionMessages.UnregisteredResource, resourceId);

                container.FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(resourceId)));
                container.ReferenceCount = 0;
                container.TriggerFailure();
                
                return container;
            }
            
            _environment.Statistics.AddReference();
            
            Log.BeginImport(_environment.Logger, container.ResourceId);
            
            container.CreateCancellationTokenSource();
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
        } catch {
            container.TriggerFailure();
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
        } catch {
            _environment.Statistics.ReleaseReferences(container.ReferenceCount);
            
            Debug.Assert(container.Status == ImportingStatus.Failed);
            throw;
        }
        
        _environment.Statistics.IncrementUniqueResourceCount();
        _cache.RegisterResourceMap(resource, container.ResourceId);

        if (context.RequestingReferences.Count == 0) {
            return new(new(container.ResourceId, resource), ReadOnlyCollection<ResourceCache.ElementContainer>.Empty);
        }

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
    }

    // Collect the references and the waiting containers.
    private async Task<ReferenceCollectResult> CollectReferences(
        object currentResource,
        ResourceCache.ElementContainer currentContainer,
        IReadOnlyDictionary<ReferencePropertyKey, RequestingReferences.RequestingReference> requestingReferences
    ) {
        _environment.Logger.LogDebug(
            "{rid}: Collect reference: {ids}",
            currentContainer.ResourceId,
            string.Join(", ", requestingReferences.Select(x => x.Value.ResourceId))
        );
        
        Dictionary<ReferencePropertyKey, ReferenceImportResult> references = [];
        HashSet<ResourceCache.ElementContainer> waitContainers = new(ReferenceContainerEqualityComparer.Instance);

        // Depth first traversing the importing graph, collect the reference containers.
        foreach ((var propertyKey, var requesting) in requestingReferences) {
            // await RecursivelyVisit(propertyKey, requesting, currentContainer);
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
            }
        }

        return new(references, waitContainers);
        
        ResourceCache.ElementContainer BeginReferenceImport(ResourceID resourceId) {
            Debug.Assert(_environment.Libraries.ContainsResource(resourceId));
            
            ResourceCache.ElementContainer container = new(resourceId);
            _environment.Statistics.AddReference();
            
            container.CreateCancellationTokenSource();
            container.ImportTask = ImportTask(container);
            container.ResolvingReferenceTask = ResolveReference(container);
            container.FinalizeTask = FinalizeTask(container);

            waitContainers.Add(container);
            
            return container;
        }
    }
    
    private async Task<ResourceHandle> FinalizeTask(ResourceCache.ElementContainer container) {
        try {
            (var handle, var waitContainers) = await container.ResolvingReferenceTask!;

            _environment.Logger.LogDebug(
                "{rid}: Wait containers: {waitContainers}",
                container.ResourceId,
                string.Join(", ", waitContainers.Select(x => x.ResourceId))
            );
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            Debug.Assert(waitContainers.All(x => x.FinalizeTask != null), "Unexpected null FinalizeTask.");
            
            await Task.WhenAll(waitContainers.Select(x => x.FinalizeTask));

            container.DisposeCancellationTokenSource();
            
            return handle;
        } catch {
            Debug.Assert(container.Status == ImportingStatus.Failed);
            throw;
        }
    }

    public ReleaseStatus Release(ResourceID resourceId) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        throw new NotImplementedException();
    }

    public ReleaseStatus Release(object resource) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        throw new NotImplementedException();
    }
    
    private void IncrementContainerReference(ResourceCache.ElementContainer container) {
        if (container.IncrementReference() != 0) {
            _environment.Statistics.AddReference();
        }
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
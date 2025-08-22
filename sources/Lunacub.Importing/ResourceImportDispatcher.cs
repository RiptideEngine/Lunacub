// ReSharper disable VariableHidesOuterVariable

using Caxivitual.Lunacub.Compilation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
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

    public ImportingOperation Import(ResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return new(Cache.GetOrBeginImporting(address, ProcessCachedContainer, BeginImport));
        
        ResourceCache.ElementContainer BeginImport(ResourceAddress address) {
            if (!TryGetImportingLibrary(address.LibraryId, out var failureContainer, out var library)) {
                return failureContainer;
            }
        
            if (!library.Registry.TryGetValue(address.ResourceId, out ResourceRegistry.Element element)) {
                string message = string.Format(ExceptionMessages.ImportFromUnregisteredResourceId, address.ResourceId, address.LibraryId);

                return new(address, string.Empty) {
                    FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(address))),
                    ReferenceCount = 0,
                    Status = ImportingStatus.Failed,
                };
            }
            
            ResourceCache.ElementContainer container = new(address, element.Name) {
                Status = ImportingStatus.Importing,
            };
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, address.LibraryId, address.ResourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);

            return container;
        }
    }

    public ImportingOperation Import(SpanNamedResourceAddress address) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return new(Cache.GetOrBeginImporting(address, ProcessCachedContainer, BeginImport));
        
        ResourceCache.ElementContainer BeginImport(SpanNamedResourceAddress address) {
            (LibraryID libraryId, ReadOnlySpan<char> name) = address;
            
            if (!TryGetImportingLibrary(libraryId, out var failureContainer, out _)) {
                return failureContainer;
            }
            
            if (!_environment.Libraries.ContainsResource(libraryId, name, out ResourceID resourceId)) {
                string message = string.Format(ExceptionMessages.ImportFromUnregisteredResourceName, name.ToString(), libraryId);

                return new(default, name.ToString()) {
                    FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(name))),
                    ReferenceCount = 0,
                    Status = ImportingStatus.Failed,
                };
            }

            ResourceCache.ElementContainer container = new(new(address.LibraryId, resourceId), name.ToString()) {
                Status = ImportingStatus.Importing,
            };
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, address.LibraryId, resourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            return container;
        }
    }

    public IReadOnlyCollection<ImportingOperation> Import(TagQuery query) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        ConcurrentBag<ImportingOperation> outputOperations = [];

        Parallel.ForEach(_environment.Libraries, EnumerateLibrary);

        return outputOperations;

        void EnumerateLibrary(ImportResourceLibrary library, ParallelLoopState state) {
            foreach ((ResourceID resourceId, ResourceRegistry.Element registryElement) in library.Registry) {
                if (!query.Check(registryElement.Tags)) continue;
                
                ResourceCache.ElementContainer container =
                    Cache.GetOrBeginImporting(new(library.Id, resourceId), ProcessCachedContainer, BeginImport, registryElement);
                
                outputOperations.Add(new(container));
            }
        }
        
        ResourceCache.ElementContainer BeginImport(ResourceAddress address, ResourceRegistry.Element element) {
            ResourceCache.ElementContainer container = new(address, element.Name) {
                Status = ImportingStatus.Importing,
            };
        
            _environment.Statistics.AddReference();
        
            Log.BeginImport(_environment.Logger, address.LibraryId, address.ResourceId);
        
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);

            return container;
        }
    }

    private bool TryGetImportingLibrary(LibraryID libraryId, [NotNullWhen(false)] out ResourceCache.ElementContainer? failureContainer, [NotNullWhen(true)] out ImportResourceLibrary? library) {
        if (libraryId == LibraryID.Null) {
            failureContainer = new(default, string.Empty) {
                FinalizeTask =
                    Task.FromException<ResourceHandle>(new ArgumentException(ExceptionMessages.ImportFromNullLibraryId, nameof(libraryId))),
                ReferenceCount = 0,
                Status = ImportingStatus.Failed,
            };
            library = null;

            return false;
        }
        
        if (!_environment.Libraries.Contains(libraryId, out library)) {
            string message = string.Format(ExceptionMessages.ImportFromUnregisteredResourceLibrary, libraryId);
            
            failureContainer = new(default, string.Empty) {
                FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(libraryId))),
                ReferenceCount = 0,
                Status = ImportingStatus.Failed,
            };
            library = null;

            return false;
        }

        failureContainer = null;
        return true;
    }
    
    private async Task<ResourceHandle> ImportingTask(ResourceCache.ElementContainer container) {
        container.EnsureCancellationTokenSourceIsNotDisposed();
        Debug.Assert(!container.CancellationToken.IsCancellationRequested);
        
        ResourceHandle handle;
        IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers;
        IReadOnlyCollection<ResourceAddress> releaseReferences;

        try {
            (handle, waitContainers, releaseReferences) = await ResolveReference(container);
        } catch (OperationCanceledException) {
            Debug.Assert(container.Status == ImportingStatus.Canceled);
            
            // Not remove from the cache to allow reimport later in the future.
            throw;
        } catch {
            Debug.Assert(container.Status == ImportingStatus.Failed);
            
            // Remove from the cache.
            bool removedSuccessfully = Cache.Remove(container.Address);
            Debug.Assert(removedSuccessfully);
            
            throw;
        }

        try {
            // Sanity check.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            Debug.Assert(waitContainers.All(x => x.FinalizeTask != null), "Unexpected null FinalizeTask.");

            _environment.Logger.LogDebug("Wait containers for resource {address}: {addresses}.", container.Address, string.Join(", ", waitContainers.Select(x => x.Address)));
            
            await Task.WhenAll(waitContainers.Select(async x => {
                try {
                    await x.FinalizeTask;
                } catch (OperationCanceledException) {
                } catch (Exception e) {
                    // TODO: Log?
                }
            }));
            
            container.CancellationToken.ThrowIfCancellationRequested();

            _environment.Statistics.IncrementUniqueResourceCount();

            Cache.RegisterResourceMap(handle.Value!, container.Address);
            container.Status = ImportingStatus.Success;

            foreach (var releasingId in releaseReferences) {
                Release(releasingId);
            }

            return handle;
        } catch (Exception e) {
            Debug.Assert(e is not OperationCanceledException, "Should not happen.");
            Debug.Assert(container.Status == ImportingStatus.Failed);

            Log.FinalizeTaskExceptionOccured(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);

            DisposeResource(handle.Value!);

            bool removedSuccessfully = Cache.Remove(container.Address);
            Debug.Assert(removedSuccessfully);

            throw;
        } finally {
            using (container.EnterLockScope()) {
                container.DisposeCancellationToken();
                container.EnsureCancellationTokenSourceIsDisposed();
            }
            
            _environment.Logger.LogDebug("End import for resource {address}.", container.Address);
        }
    }
    
    private async Task<ReferenceResolveResult> ResolveReference(ResourceCache.ElementContainer container) {
        _environment.Logger.LogDebug("Begin resolve reference for resource {address}.", container.Address);
        
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
            Log.ResolveReferenceExceptionOccured(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);
            
            Debug.Assert(container.Status == ImportingStatus.Failed);
            Debug.Assert(container.ReferenceCount == 0);
            
            throw;
        }

        try {
            ResourceHandle handle = new(container.Address, resource);

            if (context.RequestingReferences.Count == 0) {
                return new(handle, ReadOnlyCollection<ResourceCache.ElementContainer>.Empty, ReadOnlyCollection<ResourceAddress>.Empty);
            }

            container.CancellationToken.ThrowIfCancellationRequested();

            Log.BeginResolvingReference(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);

            IReadOnlyCollection<ResourceCache.ElementContainer> waitContainers =
                await ProcessReferences(container, context, resource, deserializer);

            Log.EndResolvingReference(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);

            List<ResourceAddress> releaseReferences = [];

            foreach (var releasingReference in context.RequestingReferences.ReleasedReferences) {
                bool getSuccessfully = context.RequestingReferences.References!.TryGetValue(releasingReference, out var referenceHandle);
                Debug.Assert(getSuccessfully);

                releaseReferences.Add(referenceHandle.Address);
            }

            return new(handle, waitContainers, releaseReferences);
        } catch (OperationCanceledException) {
            ReleaseContainerReferenceCounter(container);
            DisposeResource(resource);

            throw;
        } finally {
            _environment.Logger.LogDebug("End resolve reference for resource {address}.", container.Address);
        }
    }
    
    private async Task<ResourceImportResult> ImportVesselTask(ResourceCache.ElementContainer container) {
        await Task.Yield();
        
        _environment.Logger.LogDebug("Begin import vessel for resource {address}.", container.Address);

        try {
            if (_environment.Libraries.CreateResourceStream(container.Address) is not { } stream) {
                string message = string.Format(ExceptionMessages.NullResourceStream, container.Address);
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
            Log.CancelImport(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);

            using (container.EnterLockScope()) {
                ReleaseContainerReferenceCounter(container);
                container.Status = ImportingStatus.Canceled;
            }

            throw;
        } catch (Exception e) {
            Log.ReportImportException(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId, e);

            ReleaseContainerReferenceCounter(container);

            using (container.EnterLockScope()) {
                container.DisposeCancellationToken();
                container.Status = ImportingStatus.Failed;
            }

            throw;
        } finally {
            _environment.Logger.LogDebug("End import vessel for resource {address}.", container.Address);
        }
    }

    private async Task<IReadOnlyCollection<ResourceCache.ElementContainer>> ProcessReferences(ResourceCache.ElementContainer container, DeserializationContext context, object resource, Deserializer deserializer) {
        context.RequestingReferences.DisableRequest();
        
        var collectedResult = await CollectReferences(container, resource, context.RequestingReferences.Requesting);
        
        container.ReferenceResourceAddresses = collectedResult.References.Select(x => x.Value.Container.Address).ToFrozenSet();

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
        
        foreach ((var referencePropertyKey, var requesting) in requestReferences) {
            requesting.Deconstruct(out ResourceAddress requestingAddress);

            if (requestingAddress == currentContainer.Address) {
                currentContainer.IncrementReference();
                _environment.Statistics.AddReference();
                
                references.Add(referencePropertyKey, new(new(requesting.ResourceAddress, currentResource), currentContainer));
                continue;
            }
            
            if (!_environment.Libraries.ContainsResource(requesting.ResourceAddress, out ResourceRegistry.Element element)) continue;
            
            ResourceCache.ElementContainer referenceContainer =
                Cache.GetOrBeginImporting(requesting.ResourceAddress, ProcessCachedContainer, BeginReferenceImport, element.Name);
            
            try {
                (_, object vessel, _) = await referenceContainer.ImportTask!;
    
                references.Add(referencePropertyKey, new(new(requesting.ResourceAddress, vessel), referenceContainer));
            } catch {
                Debug.Assert(referenceContainer.Status == ImportingStatus.Failed);
                Debug.Assert(referenceContainer.ReferenceCount == 0);
            }
        }
        
        return new(references, waitContainers);
        
        ResourceCache.ElementContainer BeginReferenceImport(ResourceAddress resourceAddress, string? resourceName) {
            Debug.Assert(_environment.Libraries.ContainsResource(resourceAddress));
            
            Log.BeginImport(_environment.Logger, resourceAddress.LibraryId, resourceAddress.ResourceId);

            ResourceCache.ElementContainer container = new(resourceAddress, resourceName) {
                Status = ImportingStatus.Importing
            };
            _environment.Statistics.AddReference();
            
            container.InitializeImport();
            container.FinalizeTask = ImportingTask(container);
        
            waitContainers.Add(container);
            
            return container;
        }
    }
    
    private void ProcessCachedContainer(ResourceCache.ElementContainer container) {
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

                    Log.BeginImport(_environment.Logger, container.Address.LibraryId, container.Address.ResourceId);

                    container.InitializeImport();
                    container.FinalizeTask = ImportingTask(container);
                }

                break;
            
            case ImportingStatus.Failed: throw new UnreachableException();
        }
    }

    
    private void ProcessCachedContainer<T>(ResourceCache.ElementContainer container, T unusedArg) where T : allows ref struct {
        ProcessCachedContainer(container);
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
        IReadOnlyCollection<ResourceAddress> ReleaseReferences
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
            
            return x.Address.Equals(y.Address);
        }
        
        public override int GetHashCode(ResourceCache.ElementContainer container) => container.Address.GetHashCode();
    }
}
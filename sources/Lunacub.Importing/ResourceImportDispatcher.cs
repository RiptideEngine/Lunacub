using Caxivitual.Lunacub.Compilation;
using Microsoft.Extensions.Logging;

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

        ResourceCache.ElementContainer container = _cache.GetOrBeginImporting(resourceId, IncrementContainerReference, BeginImportFactory);
        return new(container);

        // ReSharper disable once VariableHidesOuterVariable
        void IncrementContainerReference(ResourceCache.ElementContainer container) {
            if (container.IncrementReference() != 0) {
                _environment.Statistics.AddReference();
            }
        }
    }

    private ResourceCache.ElementContainer BeginImportFactory(ResourceID resourceId) {
        ResourceCache.ElementContainer container = new(resourceId);

        if (!_environment.Libraries.ContainsResource(resourceId)) {
            string message = string.Format(ExceptionMessages.UnregisteredResource, resourceId);

            container.FinalizeTask = Task.FromException<ResourceHandle>(new ArgumentException(message, nameof(resourceId)));
            container.ReferenceCount = 0;
            container.TriggerFailure();
            
            return container;
        }
        
        _environment.Statistics.AddReference();
        
        container.CreateCancellationTokenSource();
        container.ImportTask = ImportTask(container);
        container.ResolvingReferenceTask = ResolveReference(container);
        container.FinalizeTask = FinalizeTask(container);
        
        return container;

        // ReSharper disable once VariableHidesOuterVariable
        async Task<ResourceImportResult> ImportTask(ResourceCache.ElementContainer container) {
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

        async Task<ResourceHandle> ResolveReference(ResourceCache.ElementContainer container) {
            object resource;
            DeserializationContext context;

            try {
                (resource, context) = await container.ImportTask!;
            } catch {
                _environment.Statistics.ReleaseReferences(container.ReferenceCount);
                
                Debug.Assert(container.Status == ImportingStatus.Failed);
                throw;
            }
            
            _environment.Statistics.IncrementUniqueResourceCount();
            
            if (context.RequestingReferences.Count == 0) return new(container.ResourceId, resource);
            
            // TODO: Implementation
            
            return new(container.ResourceId, resource);
        }
        
        async Task<ResourceHandle> FinalizeTask(ResourceCache.ElementContainer container) {
            try {
                return await container.ResolvingReferenceTask!;
            } catch {
                Debug.Assert(container.Status == ImportingStatus.Failed);
                throw;
            }
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

    public readonly record struct ResourceImportResult(object Resource, DeserializationContext Context);
}
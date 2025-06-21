using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceContainer {
    public readonly ResourceID ResourceId;

    public Task<DeserializeResult>? VesselImportTask;
    public Task<object?>? FullImportTask;
    public uint ReferenceCount;
    public readonly CancellationTokenSource CancellationTokenSource;
    
    public ResourceContainer(ResourceID resourceId) {
        ResourceId = resourceId;
        ReferenceCount = 1;
        CancellationTokenSource = new();
    }
}
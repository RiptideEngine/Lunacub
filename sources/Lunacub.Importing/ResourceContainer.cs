namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceContainer {
    public readonly ResourceID ResourceId;
    public Task<object?> FullImportTask;
    public Task<DeserializeResult> VesselImportTask;
    public uint ReferenceCount;
    public readonly CancellationTokenSource CancellationTokenSource;

    public ResourceContainer(ResourceID resourceId, uint initialReferenceCount) {
        ResourceId = resourceId;
        ReferenceCount = initialReferenceCount;
        FullImportTask = null!;
        VesselImportTask = null!;
        CancellationTokenSource = new();
    }
}
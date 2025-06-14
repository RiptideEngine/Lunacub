namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceContainer {
    public readonly ResourceID Rid;
    public Task<object?> FullImportTask;
    public Task<DeserializeResult> VesselImportTask;
    public uint ReferenceCount;
    public readonly CancellationTokenSource CancellationTokenSource;

    public ResourceContainer(ResourceID rid, uint initialReferenceCount) {
        Rid = rid;
        ReferenceCount = initialReferenceCount;
        FullImportTask = null!;
        VesselImportTask = null!;
        CancellationTokenSource = new();
    }
}
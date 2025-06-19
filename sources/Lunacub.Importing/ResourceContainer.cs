using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Importing;

internal sealed class ResourceContainer {
    public readonly ResourceID ResourceId;

    public ushort MajorVersion;
    public ushort MinorVersion;
    
    public uint ResourceBlobPosition;
    public uint OptionBlobPosition;
    public ImmutableArray<string> Tags;
    public string DeserializerName;

    public Task<DeserializeResult>? VesselImportTask;
    public Task<object?>? FullImportTask;
    public uint ReferenceCount;
    public readonly CancellationTokenSource CancellationTokenSource;
    
    public ResourceContainer(ResourceID resourceId) {
        ResourceId = resourceId;
        ReferenceCount = 1;
        CancellationTokenSource = new();
        Tags = ImmutableArray<string>.Empty;
        DeserializerName = string.Empty;
    }
    
    // public ResourceContainer(ResourceID resourceId, uint initialReferenceCount) {
    //     ResourceId = resourceId;
    //     ReferenceCount = initialReferenceCount;
    //     FullImportTask = null!;
    //     VesselImportTask = null!;
    //     CancellationTokenSource = new();
    // }
}
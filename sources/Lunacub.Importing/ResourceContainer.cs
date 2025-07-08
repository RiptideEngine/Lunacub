// using System.Collections.Frozen;
//
// namespace Caxivitual.Lunacub.Importing;
//
// internal sealed class ResourceContainer {
//     public readonly ResourceID ResourceId;
//
//     public Task<ResourceCache.VesselDeserializeResult>? VesselImportTask;
//     public Task<object>? ReferenceResolveTask;
//     public Task<object>? ReferenceWaitTask;
//     public uint ReferenceCount;
//     public FrozenSet<ResourceContainer> ReferenceContainers;
//     
//     public readonly CancellationTokenSource CancellationTokenSource;
//     
//     public ResourceContainer(ResourceID resourceId) {
//         ResourceId = resourceId;
//         ReferenceCount = 1;
//         CancellationTokenSource = new();
//         ReferenceContainers = FrozenSet<ResourceContainer>.Empty;
//     }
// }
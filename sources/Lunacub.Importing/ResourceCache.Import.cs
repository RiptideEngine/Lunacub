// // ReSharper disable VariableHidesOuterVariable
//
// using Caxivitual.Lunacub.Compilation;
// using Caxivitual.Lunacub.Exceptions;
// using Caxivitual.Lunacub.Importing.Extensions;
// using Microsoft.Extensions.Logging;
// using System.Collections.Concurrent;
// using System.Collections.Frozen;
//
// namespace Caxivitual.Lunacub.Importing;
//
// partial class ResourceCache {
//     private async Task<ResourceHandle> ImportSingleResource(ResourceID resourceId) {
//         if (await GetOrCreateResourceContainer(resourceId) is not { } container) {
//             return new(resourceId, null);
//         }
//         
//         Debug.Assert(container.ReferenceWaitTask != null);
//         
//         return new(resourceId, await container.ReferenceWaitTask);
//     }
//
//     private async Task<ResourceHandle<T>> ImportSingleResource<T>(ResourceID resourceId) where T : class {
//         if (await GetOrCreateResourceContainer(resourceId) is not { } container) {
//             return new(resourceId, null);
//         }
//         
//         Debug.Assert(container.ReferenceWaitTask != null);
//         
//         object resource = await container.ReferenceWaitTask;
//
//         return new(resourceId, resource as T);
//     }
//
//     private async Task<ResourceContainer?> GetOrCreateResourceContainer(ResourceID resourceId) {
//         if (resourceId == default) return null;
//         
//         if (!_environment.Libraries.ContainsResource(resourceId)) {
//             Log.UnregisteredResource(_environment.Logger, resourceId);
//             return null;
//         }
//         
//         await _containerLock.WaitAsync();
//         try {
//             ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, resourceId, out bool exists);
//             
//             if (exists) {
//                 Debug.Assert(container!.ReferenceCount != 0);
//         
//                 container.ReferenceCount++;
//                 Log.CachedContainer(_environment.Logger, resourceId, container.ReferenceCount);
//             } else {
//                 Log.BeginImport(_environment.Logger, resourceId);
//         
//                 container = new(resourceId);
//                 BeginImport(container);
//             }
//             
//             _environment.Statistics.AddReference();
//             return container;
//         } finally {
//             _containerLock.Release();
//         }
//     }
//     
//     private async Task GetOrCreateReferenceResourceContainers(IEnumerable<DeserializationContext.RequestingReference> requestingReferences, ) {
//         // if (!_environment.Libraries.ContainsResource(resourceId)) {
//         //     Log.UnregisteredResource(_environment.Logger, resourceId);
//         //     return;
//         // }
//         
//         await _containerLock.WaitAsync();
//         try {
//             foreach (var requestingReference in requestingReferences) {
//
//                 ref var container = ref CollectionsMarshal.GetValueRefOrAddDefault(_resourceContainers, resourceId, out bool exists);
//
//                 if (exists) {
//                     Debug.Assert(container!.ReferenceCount != 0);
//
//                     container.ReferenceCount++;
//                     Log.CachedContainer(_environment.Logger, resourceId, container.ReferenceCount);
//                 } else {
//                     Log.BeginImport(_environment.Logger, resourceId);
//
//                     container = new(resourceId);
//                     BeginImport(container, resourceType);
//                 }
//
//                 _environment.Statistics.AddReference();
//                 return container;
//             }
//         } finally {
//             _containerLock.Release();
//         }
//     }
//     
//     private void BeginImport(ResourceContainer container) {
//         container.VesselImportTask = ImportResource(container);
//         container.ReferenceResolveTask = ResolveReference(container);
//         container.ReferenceWaitTask = WaitReferencesFinish(container);
//         
//         return;
//
//         async Task<VesselDeserializeResult> ImportResource(ResourceContainer container) {
//             await Task.Yield();
//
//             if (_environment.Libraries.CreateResourceStream(container.ResourceId) is not { } resourceStream) {
//                 throw new InvalidOperationException($"Null resource stream provided despite contains resource '{container.ResourceId}'.");
//             }
//
//             BinaryHeader header;
//
//             try {
//                 header = BinaryHeader.Extract(resourceStream);
//             } catch {
//                 await resourceStream.DisposeAsync();
//                 throw;
//             }
//
//             switch (header.MajorVersion) {
//                 case 1:
//                     return await ResourceImporterVersion1.ImportVessel(
//                         _environment,
//                         resourceStream,
//                         header,
//                         container.CancellationTokenSource.Token
//                     );
//
//                 default:
//                     await resourceStream.DisposeAsync();
//
//                     string message = string.Format(
//                         ExceptionMessages.UnsupportedCompiledResourceVersion,
//                         header.MajorVersion,
//                         header.MinorVersion
//                     );
//                     throw new NotSupportedException(message);
//             }
//         }
//         
//         async Task<object> ResolveReference(ResourceContainer container) {
//             try {
//                 Debug.Assert(container.VesselImportTask != null);
//
//                 (Deserializer deserializer, object deserialized, DeserializationContext context) = await container.VesselImportTask;
//                 
//                 // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
//                 if (deserialized == null) {
//                     throw new InvalidOperationException("Deserializer cannot returns null object.");
//                 }
//
//                 bool add = _importedObjectMap.TryAdd(deserialized, container);
//                 Debug.Assert(add);
//                 
//                 _environment.Statistics.IncrementUniqueResourceCount();
//
//                 if (context.RequestingReferences.Count == 0) {
//                     _environment.Logger.LogInformation("{rid}: No reference provided.", container.ResourceId);
//                     
//                     return deserialized;
//                 }
//                 
//                 Log.BeginResolvingReference(_environment.Logger, container.ResourceId);
//
//                 // Import the references.
//                 ConcurrentDictionary<ReferencePropertyKey, (object Resource, ResourceContainer Container)> references = [];
//
//                 await Task.WhenAll(context.RequestingReferences.Select(async kvp => {
//                     DeserializationContext.RequestingReference requesting = kvp.Value;
//
//                     if (requesting.ResourceId == container.ResourceId) {
//                         container.ReferenceCount++;
//                         return;
//                     }
//                     
//                     ResourceContainer? requestingResourceContainer =
//                         await GetOrCreateResourceContainer(requesting.ResourceId, requesting.Type);
//                     
//                     if (requestingResourceContainer == null) return;
//
//                     try {
//                         object vessel = (await requestingResourceContainer.VesselImportTask!).Output;
//
//                         bool addSuccessfully = references.TryAdd(kvp.Key, (vessel, requestingResourceContainer));
//                         Debug.Assert(addSuccessfully);
//                     } catch {
//                         requestingResourceContainer.ReferenceCount--;
//                     }
//                 }));
//
//                 // Resolve references.
//                 try {
//                     context.References = references.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Resource)!;
//
//                     deserializer.ResolveReferences(deserialized, context);
//
//                     if (container.ResourceId == 3) {
//                         _environment.Logger.LogInformation("B");
//                     }
//                 } catch (Exception e) {
//                     _environment.Logger.LogError(
//                         e, 
//                         "Exception occured while resolving references for resource {rid}.",
//                         container.ResourceId
//                     );
//                 }
//
//                 container.ReferenceContainers = references.Select(kvp => kvp.Value.Container).ToFrozenSet();
//
//                 Log.EndResolvingReference(_environment.Logger, container.ResourceId);
//                 
//                 return deserialized;
//             } catch {
//                 await _containerLock.WaitAsync();
//
//                 try {
//                     _resourceContainers.Remove(container.ResourceId);
//                     _environment.Statistics.Release(container.ReferenceCount);
//                 } finally {
//                     _containerLock.Release();
//                 }
//
//                 throw;
//             } finally {
//                 container.CancellationTokenSource.Dispose();
//             }
//         }
//
//         async Task<object> WaitReferencesFinish(ResourceContainer container) {
//             object? result = await container.ReferenceResolveTask!;
//             Debug.Assert(result != null, "Unexpected null result.");
//             
//             _environment.Logger.LogInformation(
//                 "{rid}: Waiting for reference containers {references}",
//                 container.ResourceId,
//                 string.Join(", ", container.ReferenceContainers.Select(x => x.ResourceId))
//             );
//             
//             await Task.WhenAll(container.ReferenceContainers.Select(x => x.ReferenceResolveTask!));
//
//             return result;
//         }
//     }
//     
//     internal readonly record struct VesselDeserializeResult(Deserializer Deserializer, object Output, DeserializationContext Context);
// }
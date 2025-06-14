namespace Caxivitual.Lunacub.Importing;

partial class ResourceCache {
    internal uint GetReferenceCount(ResourceID rid) {
        _containerLock.Wait();

        try {
            if (!_resourceContainers.TryGetValue(rid, out ResourceContainer? value)) return 0;
            
            return value.ReferenceCount;
        } finally {
            _containerLock.Release();
        }
    }

    internal async ValueTask<uint> GetReferenceCountAsync(ResourceID rid) {
        await _containerLock.WaitAsync();
        
        try {
            if (!_resourceContainers.TryGetValue(rid, out ResourceContainer? value)) return 0;
            
            return value.ReferenceCount;
        } finally {
            _containerLock.Release();
        }
    }

    internal ResourceStatus GetResourceStatus(ResourceID rid) {
        _containerLock.Wait();

        try {
            if (!_resourceContainers.TryGetValue(rid, out ResourceContainer? value)) return ResourceStatus.NotImported;
            
            return value.FullImportTask.IsCompletedSuccessfully ? ResourceStatus.Imported : ResourceStatus.Importing;
        } finally {
            _containerLock.Release();
        }
    }
}
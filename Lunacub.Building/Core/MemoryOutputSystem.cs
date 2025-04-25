using System.Buffers;
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryOutputSystem : OutputSystem {
    public IDictionary<ResourceID, IncrementalInfo> IncrementalInfos { get; }
    public IDictionary<ResourceID, (DateTime LastBuildTime, ImmutableArray<byte> CompiledResource)> CompiledResources { get; }
    
    public MemoryOutputSystem(IDictionary<ResourceID, IncrementalInfo> incrementalInfos, IDictionary<ResourceID, (DateTime LastBuildTime, ImmutableArray<byte> CompiledResource)> compiledResources) {
        IncrementalInfos = incrementalInfos;
        CompiledResources = compiledResources;
    }
    
    public override void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver) {
        foreach ((var rid, var info) in IncrementalInfos) {
            receiver.Add(rid, info);
        }
    }
    
    public override void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports) {
        foreach ((var rid, var info) in reports) {
            IncrementalInfos[rid] = info;
        }
    }
    
    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        return CompiledResources.TryGetValue(rid, out var info) ? info.LastBuildTime : null;
    }
    
    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        byte[] buffer = new byte[sourceStream.Length - sourceStream.Position];
        sourceStream.ReadExactly(buffer);
        
        CompiledResources[rid] = (DateTime.Now, ImmutableCollectionsMarshal.AsImmutableArray(buffer));
    }
}
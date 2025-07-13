using System.Collections.Frozen;
using ResourceOutput = (System.Collections.Immutable.ImmutableArray<byte>, System.DateTime);

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryOutputSystem : OutputSystem {
    private readonly ResourceRegistry<ResourceRegistry.Element> _registry = [];
    public IReadOnlyDictionary<ResourceID, ResourceRegistry.Element> OutputRegistry => _registry;

    private readonly Dictionary<ResourceID, ResourceOutput> _outputResources = [];
    
    public IReadOnlyDictionary<ResourceID, ResourceOutput> OutputResources => _outputResources;
    
    private readonly Dictionary<ResourceID, IncrementalInfo> _incrementalInfos = [];
    public IReadOnlyDictionary<ResourceID, IncrementalInfo> IncrementalInfos => _incrementalInfos;

    public override void CollectIncrementalInfos(IDictionary<ResourceID, IncrementalInfo> receiver) {
        foreach ((var rid, var info) in _incrementalInfos) {
            receiver.Add(rid, info);
        }
    }
    
    public override void FlushIncrementalInfos(IReadOnlyDictionary<ResourceID, IncrementalInfo> reports) {
        foreach ((var rid, var report) in reports) {
            _incrementalInfos[rid] = report;
        }
    }

    public override DateTime? GetResourceLastBuildTime(ResourceID rid) {
        return _outputResources.TryGetValue(rid, out var output) ? output.Item2 : null;
    }
    
    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        byte[] buffer = new byte[sourceStream.Length];
        sourceStream.ReadExactly(buffer);
        
        _outputResources[rid] = (ImmutableCollectionsMarshal.AsImmutableArray(buffer), DateTime.Now);
    }

    public override void OutputResourceRegistry(ResourceRegistry<ResourceRegistry.Element> registry) {
        _registry.Clear();

        foreach ((var id, var element) in registry) {
            _registry.Add(id, element);
        }
    }
}
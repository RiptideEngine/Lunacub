using ResourceOutput = (System.Collections.Immutable.ImmutableArray<byte>, System.DateTime);

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryOutputSystem : OutputSystem {
    private Dictionary<ResourceID, ResourceOutput> _outputs;
    public IReadOnlyDictionary<ResourceID, ResourceOutput> Outputs => _outputs;
    
    private readonly Dictionary<ResourceID, IncrementalInfo> _incrementalInfos;
    public IReadOnlyDictionary<ResourceID, IncrementalInfo> IncrementalInfos => _incrementalInfos;

    public MemoryOutputSystem() {
        _outputs = [];
        _incrementalInfos = [];
    }

    public MemoryOutputSystem(IEnumerable<KeyValuePair<ResourceID, ResourceOutput>> outputs, IEnumerable<KeyValuePair<ResourceID, IncrementalInfo>> incrementalInfos) {
        _outputs = new(outputs);
        _incrementalInfos = new(incrementalInfos);
    }
    
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
        return _outputs.TryGetValue(rid, out var output) ? output.Item2 : null;
    }
    
    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceID rid) {
        byte[] buffer = new byte[sourceStream.Length];
        sourceStream.ReadExactly(buffer);
        
        _outputs[rid] = (ImmutableCollectionsMarshal.AsImmutableArray(buffer), DateTime.Now);
    }
}
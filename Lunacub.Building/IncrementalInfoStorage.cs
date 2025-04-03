namespace Caxivitual.Lunacub.Building;

internal sealed class IncrementalInfoStorage : IReadOnlyDictionary<ResourceID, IncrementalInfo> {
    private readonly Dictionary<ResourceID, IncrementalInfo> _reports;

    public int Count => _reports.Count;
    
    IEnumerable<ResourceID> IReadOnlyDictionary<ResourceID, IncrementalInfo>.Keys => _reports.Keys;
    IEnumerable<IncrementalInfo> IReadOnlyDictionary<ResourceID, IncrementalInfo>.Values => _reports.Values;

    internal IncrementalInfoStorage(OutputSystem outputSystem) {
        _reports = [];
        outputSystem.CollectIncrementalInfos(_reports);
    }

    internal void Add(ResourceID rid, IncrementalInfo report) {
        _reports[rid] = report;
    }

    internal bool Remove(ResourceID rid) {
        return _reports.Remove(rid);
    }

    public bool TryGet(ResourceID rid, out IncrementalInfo output) {
        return _reports.TryGetValue(rid, out output);
    }

    bool IReadOnlyDictionary<ResourceID, IncrementalInfo>.TryGetValue(ResourceID rid, out IncrementalInfo output) {
        return _reports.TryGetValue(rid, out output);
    }

    public IncrementalInfo this[ResourceID rid] => _reports[rid];

    public bool Contains(ResourceID rid) => _reports.ContainsKey(rid);
    
    bool IReadOnlyDictionary<ResourceID, IncrementalInfo>.ContainsKey(ResourceID rid) => _reports.ContainsKey(rid);

    public IEnumerator<KeyValuePair<ResourceID, IncrementalInfo>> GetEnumerator() => _reports.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_reports).GetEnumerator();
}
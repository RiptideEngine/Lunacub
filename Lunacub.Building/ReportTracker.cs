namespace Caxivitual.Lunacub.Building;

internal sealed class ReportTracker : IReadOnlyCollection<KeyValuePair<ResourceID, BuildingReport>> {
    private readonly Dictionary<ResourceID, BuildingReport> _reports;
    private readonly Dictionary<ResourceID, BuildingReport> _pendingReports;

    private readonly OutputSystem _outputSystem;
    
    public int Count => _reports.Count;
    public int PendingCount => _pendingReports.Count;

    public ReportTracker(OutputSystem outputSystem) {
        _reports = [];
        _pendingReports = [];

        _outputSystem = outputSystem;
        _outputSystem.CollectReports(_reports);
    }

    public void AddReport(ResourceID rid, BuildingReport report) {
        _reports[rid] = report;
    }

    public void AddPendingReport(ResourceID rid, BuildingReport report) {
        _pendingReports[rid] = report;
    }

    public bool RemoveReport(ResourceID rid) {
        return _pendingReports.Remove(rid) | _reports.Remove(rid);
    }

    public bool TryGetReport(ResourceID rid, out BuildingReport output) {
        return _pendingReports.TryGetValue(rid, out output) || _reports.TryGetValue(rid, out output);
    }

    public void FlushPendingReports() {
        _outputSystem.FlushReports(_pendingReports);
        
        foreach ((var rid, var report) in _pendingReports) {
            _reports[rid] = report;
        }
        
        _pendingReports.Clear();
    }
    
    public IEnumerator<KeyValuePair<ResourceID, BuildingReport>> GetEnumerator() => _reports.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_reports).GetEnumerator();
}
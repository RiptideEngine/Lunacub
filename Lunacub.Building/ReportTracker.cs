namespace Caxivitual.Lunacub.Building;

internal sealed class ReportTracker : IReadOnlyCollection<KeyValuePair<ResourceID, BuildingReport>> {
    private readonly Dictionary<ResourceID, BuildingReport> _reports;
    private readonly Dictionary<ResourceID, BuildingReport> _pendingReports;
    
    public string ReportDirectory { get; }
    
    public int Count => _reports.Count;
    public int PendingCount => _pendingReports.Count;

    public ReportTracker(string reportDirectory) {
        ReportDirectory = Path.GetFullPath(reportDirectory);
        
        if (!Directory.Exists(ReportDirectory)) {
            throw new ArgumentException($"Report directory '{ReportDirectory}' does not exist.");
        }
        
        _reports = [];
        _pendingReports = [];

        foreach (var file in Directory.EnumerateFiles(ReportDirectory, $"*{CompilingConstants.ReportExtension}", SearchOption.TopDirectoryOnly)) {
            if (!ResourceID.TryParse(Path.GetFileNameWithoutExtension(file), out var rid)) continue;

            try {
                using FileStream reportFile = File.OpenRead(file);
                
                _reports.Add(rid, JsonSerializer.Deserialize<BuildingReport>(reportFile));
            } catch {
                // Ignore any failed attempt to deserialize report.
            }
        }
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
        foreach ((var rid, var report) in _pendingReports) {
            using FileStream reportFile = File.OpenWrite(Path.Combine(ReportDirectory, $"{rid}{CompilingConstants.ReportExtension}"));
            reportFile.SetLength(0);

            JsonSerializer.Serialize(reportFile, report);
            
            _reports[rid] = report;
        }
        
        _pendingReports.Clear();
    }
    
    public IEnumerator<KeyValuePair<ResourceID, BuildingReport>> GetEnumerator() => _reports.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_reports).GetEnumerator();
}
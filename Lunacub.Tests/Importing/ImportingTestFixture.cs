namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportingTestFixture : IDisposable {
    private IReadOnlyDictionary<ResourceID, JsonObject> Resources { get; }

    public ImportingTestFixture() {
        using FileStream fs = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resources.json"));
        Resources = JsonSerializer.Deserialize<Dictionary<ResourceID, JsonObject>>(fs)!;
    }

    public void GetResourceConfiguration(BuildEnvironment environment, ResourceID rid) {
        if (!Resources.TryGetValue(rid, out JsonObject? obj) || environment.Resources.Contains(rid)) return;
        
        environment.Resources.Add(rid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", obj["Path"]!.GetValue<string>()), new() {
            ImporterName = obj[nameof(BuildingOptions.ImporterName)]!.GetValue<string>(),
            ProcessorName = obj[nameof(BuildingOptions.ProcessorName)]?.GetValue<string>(),
        });
    }
    
    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}
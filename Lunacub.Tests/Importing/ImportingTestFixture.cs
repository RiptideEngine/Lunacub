using System.Collections.Immutable;
using System.Reflection;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportingTestFixture : IDisposable {
    public IReadOnlyDictionary<Type, ImmutableArray<Type>> ComponentTypes { get; }
    private readonly IReadOnlyDictionary<ResourceID, JsonObject> _resources;

    public ImportingTestFixture() {
        using FileStream fs = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resources.json"));
        _resources = JsonSerializer.Deserialize<Dictionary<ResourceID, JsonObject>>(fs)!;

        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        ComponentTypes = new Dictionary<Type, ImmutableArray<Type>>() {
            [typeof(Importer)] = [..types.Where(x => x.IsSubclassOf(typeof(Importer)))],
            [typeof(SerializerFactory)] = [..types.Where(x => x.IsSubclassOf(typeof(SerializerFactory)))],
            [typeof(Deserializer)] = [..types.Where(x => x.IsSubclassOf(typeof(Deserializer)))],
        };
    }

    public void GetResourceConfiguration(BuildEnvironment environment, ResourceID rid) {
        if (!_resources.TryGetValue(rid, out JsonObject? obj) || environment.Resources.Contains(rid)) return;
        
        environment.Resources.Add(rid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", obj["Path"]!.GetValue<string>()), new() {
            ImporterName = obj[nameof(BuildingOptions.ImporterName)]!.GetValue<string>(),
            ProcessorName = obj[nameof(BuildingOptions.ProcessorName)]?.GetValue<string>(),
        });
    }
    
    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}
using System.Collections.Immutable;
using System.Reflection;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ResourcesFixture {
    public IReadOnlyDictionary<ResourceID, ResourceInfo> Options { get; }
    public IReadOnlyDictionary<Type, ImmutableArray<Type>> ComponentTypes { get; }
    
    public ResourcesFixture() {
        string resourceImport = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resources.json"));
        
        Options = JsonSerializer.Deserialize<Dictionary<ResourceID, ResourceInfo>>(resourceImport, new JsonSerializerOptions(JsonSerializerOptions.Default) {
            Converters = {
                new JsonStringEnumConverter(),
            },
            TypeInfoResolver = new OptionsTypeInfoResolver(),
        })!;

        List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetReferencedAssemblies().Contains(Assembly.GetExecutingAssembly().GetName()))
            .Append(Assembly.GetExecutingAssembly()).SelectMany(x => x.ExportedTypes)
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .ToList();
        
        ComponentTypes = new Dictionary<Type, ImmutableArray<Type>> {
            [typeof(Importer)] = [..types.Where(x => x.IsSubclassOf(typeof(Importer)))],
            [typeof(Processor)] = [..types.Where(x => x.IsSubclassOf(typeof(Processor)))],
            [typeof(SerializerFactory)] = [..types.Where(x => x.IsSubclassOf(typeof(SerializerFactory)))],
            [typeof(Deserializer)] = [..types.Where(x => x.IsSubclassOf(typeof(Deserializer)))],
        };
    }
    
    public void RegisterResourceToBuild(BuildEnvironment environment, ResourceID rid) {
        if (!Options.TryGetValue(rid, out ResourceInfo info) || environment.Resources.Contains(rid)) return;
        
        environment.Resources.Add(rid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", info.Path), info.Options);
    }

    public readonly record struct ResourceInfo(string Path, BuildingOptions Options);
}
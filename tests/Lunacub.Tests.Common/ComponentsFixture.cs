using System.Collections.Immutable;
using System.Reflection;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ComponentsFixture {
    public IReadOnlyDictionary<Type, ImmutableArray<Type>> ComponentTypes { get; }
    
    public ComponentsFixture() {
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

    public void ApplyComponents(BuildEnvironment environment) {
        foreach (var type in ComponentTypes[typeof(Importer)]) {
            environment.Importers[type.Name] = (Importer)Activator.CreateInstance(type)!;
        }
        
        foreach (var type in ComponentTypes[typeof(Processor)]) {
            environment.Processors[type.Name] = (Processor)Activator.CreateInstance(type)!;
        }
        
        environment.SerializerFactories.Clear();
        foreach (var type in ComponentTypes[typeof(SerializerFactory)]) {
            environment.SerializerFactories.Add((SerializerFactory)Activator.CreateInstance(type)!);
        }
    }
    
    public void ApplyComponents(ImportEnvironment environment) {
        foreach (var type in ComponentTypes[typeof(Deserializer)]) {
            environment.Deserializers[type.Name] = (Deserializer)Activator.CreateInstance(type)!;
        }
    }
}
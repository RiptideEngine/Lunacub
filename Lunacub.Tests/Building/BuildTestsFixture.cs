// ReSharper disable VirtualMemberCallInConstructor

using System.Collections.Immutable;
using System.Reflection;

namespace Caxivitual.Lunacub.Tests.Building;

public class BuildTestsFixture : ResourcesFixture {
    public IReadOnlyDictionary<Type, ImmutableArray<Type>> ComponentTypes { get; }

    public BuildTestsFixture() {
        ComponentTypes = GetComponentTypes(Assembly.GetExecutingAssembly().GetTypes());
    }

    protected virtual Dictionary<Type, ImmutableArray<Type>> GetComponentTypes(Type[] assemblyTypes) {
        return new() {
            [typeof(Importer)] = [..assemblyTypes.Where(x => x.IsSubclassOf(typeof(Importer)))],
            [typeof(SerializerFactory)] = [..assemblyTypes.Where(x => x.IsSubclassOf(typeof(SerializerFactory)))],
        };
    }
    
    public void RegisterResourceToBuild(BuildEnvironment environment, ResourceID rid) {
        if (!Options.TryGetValue(rid, out ResourceInfo info) || environment.Resources.Contains(rid)) return;
        
        environment.Resources.Add(rid, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", info.Path), info.Options);
    }
}
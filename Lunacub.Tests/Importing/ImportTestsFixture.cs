using Caxivitual.Lunacub.Tests.Building;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportTestsFixture : BuildTestsFixture {
    protected override Dictionary<Type, ImmutableArray<Type>> GetComponentTypes(Type[] assemblyTypes) {
        var componentTypes = base.GetComponentTypes(assemblyTypes);
        componentTypes.Add(typeof(Deserializer), [..assemblyTypes.Where(x => x.IsSubclassOf(typeof(Deserializer)))]);
        
        return componentTypes;
    }
}
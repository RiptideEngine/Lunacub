using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportingTestFixture : IDisposable {
    public IReadOnlyDictionary<Type, ImmutableArray<Type>> ComponentTypes { get; }
    private readonly Dictionary<ResourceID, JsonObject> _resources;

    public ImportingTestFixture() {
        _resources = JsonSerializer.Deserialize<Dictionary<ResourceID, JsonObject>>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resources.json")), new JsonSerializerOptions {
            Converters = {
                new JsonStringEnumConverter(),
            },
            TypeInfoResolver = new OptionsTypeInfoResolver(),
        })!;

        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        ComponentTypes = new Dictionary<Type, ImmutableArray<Type>> {
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
    
    private sealed class OptionsTypeInfoResolver : DefaultJsonTypeInfoResolver {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            if (jsonTypeInfo.Type == typeof(object)) {
                jsonTypeInfo.PolymorphismOptions = new ()
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes = {
                        new(typeof(OptionsResourceDTO.Options), "OptionsResource.Options"),
                    }
                };
            }

            return jsonTypeInfo;
        }
    }
}
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Tests.Importing;

public sealed class ImportEnvironmentTests_Async {
    [Fact]
    public async Task ImportTasks_SimpleResource_CorrectlyImport() {
        Dictionary<ResourceID, (DateTime, ImmutableArray<byte>)> compiledResources = [];
        
        using BuildEnvironment buildEnv = new(new MemoryOutputSystem(new Dictionary<ResourceID, IncrementalInfo>(), compiledResources));
        buildEnv.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        buildEnv.SerializerFactories.Add(new SimpleResourceSerializerFactory());
        buildEnv.Resources.Add(new("e0b8066bf60043c5a0c3a7782363427d"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SimpleResource.json"), new(nameof(SimpleResourceImporter)));

        var result = buildEnv.BuildResources();
        result.ResourceResults.Should().ContainSingle().Which.Key.Should().Be(new ResourceID("e0b8066bf60043c5a0c3a7782363427d"));
        compiledResources.Should().ContainSingle().Which.Key.Should().Be(new ResourceID("e0b8066bf60043c5a0c3a7782363427d"));

        var compiledResource = compiledResources.First().Value.Item2;
        
        using ImportEnvironment importEnv = new() {
            Input = {
                Libraries = {
                    new MemoryResourceLibrary(Guid.NewGuid(), Enumerable.Range(1, 100).Select(i => KeyValuePair.Create(new ResourceID((UInt128)i), compiledResource)).ToDictionary()),
                },
            },
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
            },
        };

        // importEnv.Deserializers.Should().ContainKey(nameof(SimpleResourceDeserializer)).WhoseValue.Should().BeOfType<SimpleResourceDeserializer>();

        await Task.WhenAll(Enumerable.Range(1, 100).Select(i => {
            ResourceID rid = new((UInt128)i);
            return importEnv.ImportAsync<SimpleResource>(rid).Task;
        }).ToArray());
        
        importEnv.Statistics.TotalReferenceCount.Should().Be(100);
        importEnv.Statistics.UniqueResourceCount.Should().Be(100);
    }
}
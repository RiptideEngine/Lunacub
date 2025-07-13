using BenchmarkDotNet.Attributes;
using Benchmarks.Common;
using BuildMemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;
using ImportMemorySourceProvider = Caxivitual.Lunacub.Importing.Core.MemorySourceProvider;

namespace Benchmarks.Importing;

public class SimpleResourceImporting {
    [Params(10, 100, 1000)]
    public int Count { get; set; }

    public ImportEnvironment _importEnvironment = null!;

    private List<Task> _tasks;

    [GlobalSetup]
    public void BuildResources() {
        var buildOutput = new Caxivitual.Lunacub.Building.Core.MemoryOutputSystem();
        
        var buildSourceProvider = new BuildMemorySourceProvider();
        var buildLibrary = new BuildResourceLibrary(buildSourceProvider);

        for (uint i = 1; i <= Count; i++) {
            string name = $"Resource{i}";
            
            buildSourceProvider.Sources
                .Add(name, BuildMemorySourceProvider.AsUtf8($$"""{"Value":{{i}}}""", DateTime.MinValue));
            
            buildLibrary.Registry.Add(i, new(name, [], new() {
                Addresses = new(name),
                Options = new(nameof(SimpleResourceImporter)),
            }));
        }
        
        using BuildEnvironment buildEnv = new BuildEnvironment(buildOutput) {
            Importers = {
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
            },
            Libraries = {
                buildLibrary,
            },
        };
        
        buildEnv.BuildResources();

        var importSourceProvider = new ImportMemorySourceProvider();
        var importLibrary = new ImportResourceLibrary(importSourceProvider);

        for (uint i = 1; i <= Count; i++) {
            ResourceID id = i;
            
            importSourceProvider.Resources.Add(id, buildOutput.OutputResources[id].Item1);
            importLibrary.Registry.Add(id, buildOutput.OutputRegistry[id]);
        }

        _importEnvironment = new() {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
            },
            Libraries = {
                importLibrary,
            },
        };

        _tasks = new(Count);
    }
    
    [Benchmark]
    public async Task Import() {
        for (uint i = 1; i <= Count; i++) {
            _tasks.Add(_importEnvironment.Import(i).Task);
        }

        await Task.WhenAll(_tasks);
    }
}
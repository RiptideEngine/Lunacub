using Caxivitual.Lunacub.Building.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

internal static class Program {
    private static readonly ILogger _logger = LoggerFactory.Create(builder => {
        builder.AddConsole();
    }).CreateLogger("Program");
    
    private static async Task Main(string[] args) {
        string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Reports");
        string resOutputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Resources");
        
        Directory.CreateDirectory(reportDir);
        Directory.CreateDirectory(resOutputDir);
        
        BuildResources(reportDir, resOutputDir);
        await ImportResource(resOutputDir);
    }
    
    private static void BuildResources(string reportDirectory, string outputDirectory) {
        _logger.LogInformation("Building resources...");
        
        using BuildEnvironment env = new(new FileOutputSystem(reportDirectory, outputDirectory)) {
            Logger = _logger,
            Importers = {
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
                [nameof(MergingResourceImporter)] = new MergingResourceImporter(),
            },
            Processors = {
                [nameof(MergingResourceProcessor)] = new MergingResourceProcessor(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
                new MergingResourceSerializerFactory(),
            },
            Libraries = {
                new(new MemorySourceProvider {
                    Sources = {
                        ["Resource1"] = MemorySourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
                        ["Resource2"] = MemorySourceProvider.AsUtf8("""{"Value":2}""", DateTime.MinValue),
                        ["Resource3"] = MemorySourceProvider.AsUtf8("""{"Value":3}""", DateTime.MinValue),
                        ["MergingResource"] = MemorySourceProvider.AsUtf8("""{"Dependencies":[1,2,3]}""", DateTime.MinValue),
                    },
                }) {
                    Registry = {
                        [1] = new("Resource1", [], new() {
                            Addresses = new("Resource1"),
                            Options = new(nameof(SimpleResourceImporter)),
                        }),
                        [2] = new("Resource2", [], new() {
                            Addresses = new("Resource2"),
                            Options = new(nameof(SimpleResourceImporter)),
                        }),
                        [3] = new("Resource3", [], new() {
                            Addresses = new("Resource3"),
                            Options = new(nameof(SimpleResourceImporter)),
                        }),
                        [4] = new("MergeingResource", [], new() {
                            Addresses = new("MergingResource"),
                            Options = new(nameof(MergingResourceImporter), nameof(MergingResourceProcessor)),
                        }),
                    },
                },
            },
        };

        var result = env.BuildResources();
        
        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            if (resourceResult.IsSuccess) {
                _logger.LogInformation("Resource {rid} build status: {status}.", rid, resourceResult.Status);
            } else {
                _logger.LogError(resourceResult.Exception?.SourceException, "Resource {rid} build status: {status}.", rid, resourceResult.Status);
            }
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
                [nameof(MergingResourceDeserializer)] = new MergingResourceDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                new(new FileSourceProvider(resourceDirectory)) {
                    Registry = {
                        [4] = new("Resource", []),
                    },
                },
            },
        };

        ResourceHandle<MergingResource> handle = (await importEnvironment.Import(4)).Convert<MergingResource>();
        
        _logger.LogInformation("Values: {values}", string.Join(", ", handle.Value!.Values));

        await Task.Delay(100);
    }
}
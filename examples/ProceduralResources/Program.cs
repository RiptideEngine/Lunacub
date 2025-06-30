using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

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
            Importers = {
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
                [nameof(EmittableResourceImporter)] = new EmittableResourceImporter(),
            },
            Processors = {
                [nameof(EmittableResourceProcessor)] = new EmittableResourceProcessor(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
                new EmittableResourceSerializerFactory(),
            },
            Libraries = {
                new(new MemorySourceProvider() {
                    Sources = {
                        ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
                    },
                }) {
                    Registry = {
                        [1] = new("Resource", [], new() {
                            Addresses = new("Resource"),
                            Options = new(nameof(EmittableResourceImporter), nameof(EmittableResourceProcessor)),
                        }),
                    },
                },
            },
        };
        
        _logger.LogInformation("Version: {importer}, {processor}", env.Importers[nameof(EmittableResourceImporter)].Version, env.Processors[nameof(EmittableResourceProcessor)].Version);
        
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
                [nameof(EmittableResourceDeserializer)] = new EmittableResourceDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                new(new FileSourceProvider(resourceDirectory)) {
                    Registry = {
                        [1] = new("Resource", [], 0),
                    },
                },
            },
        };
        
        ResourceHandle<EmittableResource> handle = await importEnvironment.Import<EmittableResource>(1).Task;
        
        _logger.LogInformation("resource.Value: {value}", handle.Value!.Value);
        _logger.LogInformation("resource.Value.Generated.Value: {value}", handle.Value!.Generated!.Value);
    }
}
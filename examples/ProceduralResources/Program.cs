using Caxivitual.Lunacub.Building.Collections;
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

internal static class Program {
    private static readonly ILogger _logger = LoggerFactory.Create(builder => {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Debug);
    }).CreateLogger("Program");

    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    
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
        
        using BuildEnvironment env = new(new FileOutputSystem(reportDirectory, outputDirectory), _memoryStreamManager) {
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
                new(1, new MemorySourceProvider {
                    Sources = {
                        ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":16}""", DateTime.MinValue),
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
            Logger = _logger,
        };
        
        env.BuildResources();
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        string libraryDirectory = Path.Combine(resourceDirectory, "1");
        ImportResourceLibrary library = new(1, new FileSourceProvider(libraryDirectory));

        using (var registryStream = File.OpenRead(Path.Combine(libraryDirectory, "__registry"))) {
            foreach ((var resourceId, var element) in JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element>>(registryStream)!) {
                library.Registry.Add(resourceId, element);
            }
        }
        
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
                [nameof(EmittableResourceDeserializer)] = new EmittableResourceDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                library,
            },
        };
        
        // TODO: Fix caching procedural resource make registry disappear
        
        ResourceHandle<EmittableResource> handle = (await importEnvironment.Import(1, 1)).Convert<EmittableResource>();
        
        _logger.LogInformation("resource.Value: {value}", handle.Value!.Value);
        _logger.LogInformation("resource.Value.Generated.Value: {value}", handle.Value!.Generated!.Value);
    }
}
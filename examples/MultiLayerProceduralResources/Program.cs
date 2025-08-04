using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text.Json;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace MultiLayerProceduralResources;

internal static class Program {
    private static readonly ILogger _logger = LoggerFactory.Create(builder => {
        builder.AddConsole();
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
                [nameof(EmittingResourceImporter)] = new EmittingResourceImporter(),
            },
            Processors = {
                [nameof(EmittingResourceProcessor)] = new EmittingResourceProcessor(),
            },
            SerializerFactories = {
                new EmittingResourceSerializerFactory(),
            },
            Libraries = {
                new(1, new MemorySourceProvider {
                    Sources = {
                        ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":1,"Count":5}""", DateTime.MinValue),
                    },
                }) {
                    Registry = {
                        [1] = new("Resource", [], new() {
                            Addresses = new("Resource"),
                            Options = new(nameof(EmittingResourceImporter), nameof(EmittingResourceProcessor)),
                        }),
                    },
                },
            },
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
                [nameof(EmittingResourceDeserializer)] = new EmittingResourceDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                library,
            },
        };
        
        ResourceHandle<EmittingResource> handle = (await importEnvironment.Import(new(1, 1))).Convert<EmittingResource>();
        
        _logger.LogInformation("Imported: {value}.", handle.Value);
    }
}
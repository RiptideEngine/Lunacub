using Caxivitual.Lunacub.Building.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text.Json;
using FileSourceRepository = Caxivitual.Lunacub.Importing.Core.FileSourceRepository;
using MemorySourceRepository = Caxivitual.Lunacub.Building.Core.MemorySourceRepository;

namespace MultiLayerProceduralResources;

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
        
        using BuildEnvironment env = new(new FileResourceSink(outputDirectory), new FileBuildCacheIO(reportDirectory), _memoryStreamManager) {
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
                new(1, new MemorySourceRepository {
                    Sources = {
                        ["Resource"] = MemorySourceRepository.AsUtf8("""{"Value":1,"Count":5}""", DateTime.MinValue),
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

        env.BuildResources(BuildFlags.Rebuild);
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        string libraryDirectory = Path.Combine(resourceDirectory, "1");
        ImportResourceLibrary library = new(1, new FileSourceRepository(libraryDirectory));

        using (var registryStream = File.OpenRead(Path.Combine(libraryDirectory, "__registry"))) {
            foreach ((var resourceId, var element) in JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element>>(registryStream)!) {
                library.Registry.Add(resourceId, element);
            }
        }
        
        using ImportEnvironment importEnvironment = new ImportEnvironment(_memoryStreamManager);
        importEnvironment.Deserializers[nameof(EmittingResourceDeserializer)] = new EmittingResourceDeserializer();
        importEnvironment.Logger = _logger;
        importEnvironment.Libraries.Add(library);

        ResourceHandle<EmittingResource> handle = (await importEnvironment.Import(1, 1)).Convert<EmittingResource>();
        
        _logger.LogInformation("Imported: {value}.", handle.Value);
    }
}
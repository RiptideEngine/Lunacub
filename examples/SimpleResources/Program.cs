using Caxivitual.Lunacub.Building.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using FileSourceRepository = Caxivitual.Lunacub.Importing.Core.FileSourceRepository;
using MemorySourceRepository = Caxivitual.Lunacub.Building.Core.MemorySourceRepository;

namespace Caxivitual.Lunacub.Examples.SimpleResources;

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

        using BuildEnvironment env = new(new FileResourceSink(reportDirectory, outputDirectory), _memoryStreamManager) {
            Importers = {
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
            },
            Libraries = {
                new(1, new MemorySourceRepository {
                    Sources = {
                        ["PrimaryResource"] = MemorySourceRepository.AsUtf8("""{"Value":1}""", DateTime.MinValue),
                    },
                }) {
                    Registry = {
                        [1] = new("Resource", [], new() {
                            Addresses = new("PrimaryResource"),
                            Options = new(nameof(SimpleResourceImporter)),
                        }),
                    },
                },
            },
        };
        
        env.BuildResources(BuildFlags.Rebuild);
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment(_memoryStreamManager);
        importEnvironment.Deserializers[nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer();
        importEnvironment.Logger = _logger;
        importEnvironment.Libraries.Add(new(1, new FileSourceRepository(Path.Combine(resourceDirectory, "1"))) {
            Registry = {
                [1] = new("Resource", []),
            },
        });

        ResourceHandle<SimpleResource> handle = (await importEnvironment.Import(1, 1).Task).Convert<SimpleResource>();
        
        _logger.LogInformation("Imported: {value}.", handle.Value);
    }
}
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text;
using System.Text.Json;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Examples.SimpleResources;

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
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
            },
            Libraries = {
                new(1, new MemorySourceProvider {
                    Sources = {
                        ["PrimaryResource"] = MemorySourceProvider.AsUtf8("""{"Value":16}""", DateTime.MinValue),
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

        var result = env.BuildResources();

        foreach ((var libraryId, var libraryResults) in result.EnvironmentResults) {
            foreach ((var resourceId, var resourceResult) in libraryResults) {
                if (resourceResult.IsSuccess) {
                    _logger.LogInformation("Resource {rid} of library {lid} build status: {status}.", resourceId, libraryId, resourceResult.Status);
                } else {
                    _logger.LogError(resourceResult.Exception?.SourceException, "Resource {rid} of library {lib} build status: {status}.", resourceId, libraryId, resourceResult.Status);
                }
            }
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment();
        importEnvironment.Deserializers[nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer();
        importEnvironment.Logger = _logger;
        importEnvironment.Libraries.Add(new(1, new FileSourceProvider(resourceDirectory)) {
            Registry = {
                [1] = new("Resource", []),
            },
        });

        ResourceHandle<SimpleResource> handle = (await importEnvironment.Import(new(1, 1)).Task).Convert<SimpleResource>();
        
        _logger.LogInformation("Imported: {value}.", handle.Value);
    }
}
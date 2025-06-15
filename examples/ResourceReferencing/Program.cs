using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

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
                [nameof(ReferencingResourceImporter)] = new ReferencingResourceImporter(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
                new ReferencingResourceSerializerFactory(),
            },
            Resources = {
                [1] = new() {
                    Provider = MemoryResourceProvider.AsUtf8("""{"ReferenceId":2}""", DateTime.MinValue),
                    Options = new() {
                        ImporterName = nameof(ReferencingResourceImporter),
                    },
                },
                [2] = new() {
                    Provider = MemoryResourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
                    Options = new() {
                        ImporterName = nameof(SimpleResourceImporter),
                    },
                },
            },
        };

        var result = env.BuildResources();

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            _logger.LogError(resourceResult.Exception?.SourceException, "Resource '{rid}' build status: {status}.", rid, resourceResult.Status);
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        _logger.LogInformation("Importing resources...");
        
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
                [nameof(ReferencingResourceDeserializer)] = new ReferencingResourceDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                new FileResourceLibrary(resourceDirectory),
            }
        };
        
        ResourceHandle<ReferencingResource> handle = await importEnvironment.Import<ReferencingResource>(1).Task;
        
        _logger.LogInformation("Reference value: {value}.", handle.Value!.Reference!.Value);
    }
}
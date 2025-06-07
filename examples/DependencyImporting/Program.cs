using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

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
            Resources = {
                [1] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource1.json")),
                    Options = new() {
                        ImporterName = nameof(SimpleResourceImporter),
                    },
                },
                [2] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource2.json")),
                    Options = new() {
                        ImporterName = nameof(SimpleResourceImporter),
                    },
                },
                [3] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource3.json")),
                    Options = new() {
                        ImporterName = nameof(SimpleResourceImporter),
                    },
                },
                [4] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MergingResource.json")),
                    Options = new() {
                        ImporterName = nameof(MergingResourceImporter),
                        ProcessorName = nameof(MergingResourceProcessor),
                    },
                },
            },
        };

        var result = env.BuildResources();
        
        Debug.Assert(result.ResourceResults.Count == 4);

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            _logger.LogInformation("Resource '{rid}' build status: {status}.", rid, resourceResult.Status);
            resourceResult.Exception?.Throw();
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
                new FileResourceLibrary(resourceDirectory),
            },
        };

        ResourceHandle<MergingResource> handle = await importEnvironment.Import<MergingResource>(4).Task;
        
        _logger.LogInformation("Values: {values}", string.Join(", ", handle.Value!.Values));

        await Task.Delay(100);
    }
}
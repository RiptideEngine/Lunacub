using Caxivitual.Lunacub.Building.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            Resources = {
                [1] = new() {
                    Provider = MemoryResourceProvider.AsUtf8("""{"Value":1}""", DateTime.MinValue),
                    Options = new() {
                        ImporterName = nameof(EmittableResourceImporter),
                        ProcessorName = nameof(EmittableResourceProcessor),
                    },
                },
            },
        };
        
        var result = env.BuildResources();
        
        Debug.Assert(result.ResourceResults.Count == 2, "Expected 2 results, got " + result.ResourceResults.Count + '.');

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            _logger.LogInformation("Resource '{rid}' build status: {status}.", rid, resourceResult.Status);
            resourceResult.Exception?.Throw();
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        // using ImportEnvironment importEnvironment = new ImportEnvironment() {
        //     Deserializers = {
        //         [nameof(ReferenceResourceDeserializer)] = new ReferenceResourceDeserializer(),
        //     },
        //     Logger = _logger,
        //     Libraries = {
        //         new FileResourceLibrary(resourceDirectory)
        //     },
        // };
        //
        // ResourceHandle<ReferenceResource> handle = await importEnvironment.Import<ReferenceResource>(1).Task;
        //
        // _logger.LogInformation("resource.Value: {value}", handle.Value!.Value);
        // _logger.LogInformation("resource.Reference.Value: {refValue}", handle.Value!.Reference!.Value);
        // _logger.LogInformation("resource.Reference.Reference.Value: {referefValue}", handle.Value!.Reference!.Reference!.Value);
        //
        // await Task.Delay(100);
    }
}
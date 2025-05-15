using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ReferenceImporting;

internal static class Program {
    private static readonly ILogger _logger = LoggerFactory.Create(builder => {
        builder.AddSimpleConsole(options => {
            options.SingleLine = true;
        });
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
                [nameof(ReferenceResourceImporter)] = new ReferenceResourceImporter(),
            },
            SerializerFactories = {
                new ReferenceResourceSerializerFactory(),
            },
            Resources = {
                [1] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource1.json")),
                    Options = new() {
                        ImporterName = nameof(ReferenceResourceImporter),
                    },
                },
                [2] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource2.json")),
                    Options = new() {
                        ImporterName = nameof(ReferenceResourceImporter),
                    },
                },
                [3] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource3.json")),
                    Options = new() {
                        ImporterName = nameof(ReferenceResourceImporter),
                    },
                },
            },
        };

        var result = env.BuildResources();
        
        Debug.Assert(result.ResourceResults.Count == 3);

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            _logger.LogInformation("Resource '{rid}' build status: {status}.", rid, resourceResult.Status);
            resourceResult.Exception?.Throw();
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment();
        importEnvironment.Deserializers[nameof(ReferenceResourceDeserializer)] = new ReferenceResourceDeserializer();
        importEnvironment.Logger = _logger;
        importEnvironment.Libraries.Add(new FileResourceLibrary(resourceDirectory));

        ResourceHandle<ReferenceResource> handle = await importEnvironment.ImportAsync<ReferenceResource>(1).Task;
        
        _logger.LogInformation("resource.Value: {value}", handle.Value!.Value);
        _logger.LogInformation("resource.Reference.Value: {refValue}", handle.Value!.Reference!.Value);
        _logger.LogInformation("resource.Reference.Reference.Value: {referefValue}", handle.Value!.Reference!.Reference!.Value);

        await Task.Delay(100);
    }
}
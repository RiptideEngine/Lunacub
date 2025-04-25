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
        };
        
        env.Resources.Add(new("01965cef1af37ee9bd5d6c37ae77bbf0"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource1.json"), new(nameof(ReferenceResourceImporter)));
        env.Resources.Add(new("01965cecccaa74028f6e0ae1095dc388"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource2.json"), new(nameof(ReferenceResourceImporter)));
        env.Resources.Add(new("019661146647790c960521de9fda2773"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource3.json"), new(nameof(ReferenceResourceImporter)));

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
        importEnvironment.Input.Libraries.Add(new FileResourceLibrary(Guid.NewGuid(), resourceDirectory));

        ResourceHandle<ReferenceResource> handle = await importEnvironment.ImportAsync<ReferenceResource>(new("01965cef1af37ee9bd5d6c37ae77bbf0")).Task;
        
        Console.WriteLine("resource.Value: " + handle.Value!.Value);
        Console.WriteLine("resource.Reference.Value: " + handle.Value!.Reference!.Value);
        Console.WriteLine("resource.Reference.Reference.Value: " + handle.Value!.Reference!.Reference!.Value);
    }
}
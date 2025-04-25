using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using System.Diagnostics;

namespace Caxivitual.Lunacub.Examples.SimpleResourceImporting;

internal static class Program {
    private static async Task Main(string[] args) {
        string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Reports");
        string resOutputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Resources");
        
        Directory.CreateDirectory(reportDir);
        Directory.CreateDirectory(resOutputDir);
        
        BuildResources(reportDir, resOutputDir);
        await ImportResource(resOutputDir);
    }

    private static void BuildResources(string reportDirectory, string outputDirectory) {
        Console.WriteLine("Building resources...");

        using BuildEnvironment env = new(new FileOutputSystem(reportDirectory, outputDirectory)) {
            Importers = {
                [nameof(SimpleResourceImporter)] = new SimpleResourceImporter(),
            },
            SerializerFactories = {
                new SimpleResourceSerializerFactory(),
            },
        };
        
        env.Resources.Add(new("d2bb9aa4d1a9443489e0434885d12d97"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource.json"), new(nameof(SimpleResourceImporter)));

        var result = env.BuildResources();

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            Console.WriteLine($"Resource '{rid}' build status: {resourceResult.Status}.");
            resourceResult.Exception?.Throw();
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
            },
        };
        importEnvironment.Input.Libraries.Add(new FileResourceLibrary(Guid.NewGuid(), resourceDirectory));
        
        ResourceHandle<SimpleResource> handle = await importEnvironment.ImportAsync<SimpleResource>(new("d2bb9aa4d1a9443489e0434885d12d97")).Task;
        
        Console.WriteLine("Imported: " + handle.Value);
    }
}
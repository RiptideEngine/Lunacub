using System.Diagnostics;

namespace Caxivitual.Lunacub.Examples.SimpleResource;

class Program {
    private static ResourceID ResourceID = new("d2bb9aa4d1a9443489e0434885d12d97");
    
    private static async Task Main(string[] args) {
        string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Reports");
        string resOutputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs", "Resources");
        
        Directory.CreateDirectory(reportDir);
        Directory.CreateDirectory(resOutputDir);
        
        BuildResources(reportDir, resOutputDir);

        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
            },
        };
        importEnvironment.Input.Libraries.Add(new FileResourceLibrary(Guid.NewGuid(), resOutputDir));

        ResourceHandle handle = await importEnvironment.ImportAsync(ResourceID);
        
        Debug.Assert(handle.Value is SimpleResource, $"Expecting SimpleResource, {handle.Value.GetType().FullName} imported.");
        
        SimpleResource resource = (SimpleResource)handle.Value;
        
        Console.WriteLine("Integer: " + resource.Integer);
        Console.WriteLine("Single: " + resource.Single);
        Console.WriteLine("Vector: " + resource.Vector);
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
        
        env.Resources.Add(ResourceID, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource.json"), new(nameof(SimpleResourceImporter)));

        var result = env.BuildResources();

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            Console.WriteLine($"Resource '{rid}' build status: {resourceResult.Status}.");
            resourceResult.Exception?.Throw();
        }
    }
}
using System.Diagnostics;

namespace ReferenceImporting;

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
                [nameof(ReferenceResourceImporter)] = new ReferenceResourceImporter(),
            },
            SerializerFactories = {
                new ReferenceResourceSerializerFactory(),
            },
        };
        
        env.Resources.Add(new("01965cef1af37ee9bd5d6c37ae77bbf0"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource1.json"), new(nameof(ReferenceResourceImporter)));
        env.Resources.Add(new("01965cecccaa74028f6e0ae1095dc388"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resource2.json"), new(nameof(ReferenceResourceImporter)));

        var result = env.BuildResources();
        
        Debug.Assert(result.ResourceResults.Count == 2);

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            Console.WriteLine($"Resource '{rid}' build status: {resourceResult.Status}.");
            resourceResult.Exception?.Throw();
        }
    }
    
    private static async Task ImportResource(string resourceDirectory) {
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(ReferenceResourceDeserializer)] = new ReferenceResourceDeserializer(),
            },
        };
        importEnvironment.Input.Libraries.Add(new FileResourceLibrary(Guid.NewGuid(), resourceDirectory));

        ResourceHandle handle = await importEnvironment.ImportAsync(new("01965cef1af37ee9bd5d6c37ae77bbf0")).Task;
        
        Debug.Assert(handle.Value is ReferenceResource, $"Expecting SimpleResource, {handle.Value.GetType().FullName} imported.");
        
        ReferenceResource resource = (ReferenceResource)handle.Value;
        
        Console.WriteLine("resource.Value: " + resource.Value);
        Console.WriteLine("resource.Reference.Value: " + resource.Reference!.Value);
    }
}
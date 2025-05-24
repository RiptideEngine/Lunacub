using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

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
                [nameof(TextureImporter)] = new TextureImporter(),
                [nameof(TextureAtlasImporter)] = new TextureAtlasImporter(),
            },
            Processors = {
                [nameof(TextureAtlasProcessor)] = new TextureAtlasProcessor(),
            },
            SerializerFactories = {
                new TextureSerializerFactory(),
                new TextureAtlasSerializerFactory(),
            },
            Resources = {
                [1] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Texture1.json")),
                    Options = new() {
                        ImporterName = nameof(TextureImporter)
                    },
                },
                [2] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Texture2.json")),
                    Options = new() {
                        ImporterName = nameof(TextureImporter)
                    },
                },
                [3] = new() {
                    Provider = new FileResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Atlas.json")),
                    Options = new() {
                        ImporterName = nameof(TextureAtlasImporter),
                        ProcessorName = nameof(TextureAtlasProcessor),
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
        using ImportEnvironment importEnvironment = new ImportEnvironment {
            Deserializers = {
                [nameof(TextureDeserializer)] = new TextureDeserializer(),
                [nameof(TextureAtlasDeserializer)] = new TextureAtlasDeserializer(),
            },
            Logger = _logger,
            Libraries = {
                new FileResourceLibrary(resourceDirectory)
            }
        };
        
        ResourceHandle<TextureAtlas> handle = await importEnvironment.Import<TextureAtlas>(3).Task;
        
        _logger.LogInformation("Imported: {value}.", handle.Value.Name);
    }
}
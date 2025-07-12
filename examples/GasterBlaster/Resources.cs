using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public static class Resources {
    private static bool _initialized;

    private static Renderer _renderer = null!;
    private static ILogger _logger = null!;
    
    private static ImportEnvironment _importEnv = null!;
    
    public static void Initialize(Renderer renderer, ILogger logger) {
        if (Interlocked.Exchange(ref _initialized, true)) {
            throw new InvalidOperationException("Resources has already been initialized.");
        }

        _renderer = renderer;
        _logger = logger;

        string compiledResourceDirectory = BuildResources();
        CreateImportEnvironment(compiledResourceDirectory);
    }
    
    private static string BuildResources() {
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource Outputs");
        string reportDirectory = Path.Combine(outputDirectory, "Reports");
        string resourcesDirectory = Path.Combine(outputDirectory, "Resources");

        Directory.CreateDirectory(reportDirectory);
        Directory.CreateDirectory(resourcesDirectory);
        
        using BuildEnvironment environment = new BuildEnvironment(new FileOutputSystem(reportDirectory, resourcesDirectory)) {
            Importers = {
                [nameof(SpriteImporter)] = new SpriteImporter(),
                [nameof(Texture2DImporter)] = new Texture2DImporter(),
            },
            Processors = {
            },
            SerializerFactories = {
                new SpriteSerializerFactory(),
                new Texture2DSerializerFactory(),
            },
            Logger = _logger,
        };
        
        string resourceDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

        environment.Libraries.Add(new(new Building.Core.FileSourceProvider(resourceDirectoryPath)) {
            Registry = {
                [1] = new("Blaster Texture", [], new() {
                    Addresses = new("gaster_blaster.png"),
                    Options = new(nameof(Texture2DImporter)),
                }),
                [2] = new("Blaster Sprite", [], new() {
                    Addresses = new("gaster_blaster.sprjson"),
                    Options = new(nameof(SpriteImporter)),
                }),
                [3] = new("Blaster Ray Texture", [], new() {
                    Addresses = new("gaster_blaster_ray.png"),
                    Options = new(nameof(Texture2DImporter)),
                }),
                [4] = new("Blaster Ray Sprite", [], new() {
                    Addresses = new("gaster_blaster_ray.sprjson"),
                    Options = new(nameof(SpriteImporter)),
                }),
            },
        });

        var result = environment.BuildResources();

        StringBuilder sb = new StringBuilder(1024);
        sb.AppendLine("Resource building result:");
        sb.Append("- Resource Count: ").Append(result.ResourceResults.Count).AppendLine(".");
        sb.Append("- Times: ").Append(result.BuildStartTime).Append(" - ").Append(result.BuildFinishTime).AppendLine(".");
        sb.AppendLine("- Results:");

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            sb.Append("  - ").Append($"{rid:X}");
            
            if (resourceResult.Exception == null) {
                sb.Append(": ").Append(resourceResult.Status).AppendLine(".");
            } else {
                sb.AppendLine(":").Append("    - Status: ").Append(resourceResult.Status).AppendLine(".");
                sb.Append("    - Exception: ").Append(resourceResult.Exception.SourceException.Message).AppendLine(".");
            }
        }
        
        _logger.LogInformation("{text}", sb.ToString());

        return resourcesDirectory;
    }

    private static void CreateImportEnvironment(string compiledResourceDirectory) {
        _importEnv = new() {
            Deserializers = {
                [nameof(SpriteDeserializer)] = new SpriteDeserializer(),
                [nameof(Texture2DDeserializer)] = new Texture2DDeserializer(_renderer),
            },
            Disposers = {
                Disposer.Create(obj => {
                    if (obj is not IDisposable disposable) return false;

                    disposable.Dispose();
                    return true;
                }),
            },
            Libraries = {
                new(new FileSourceProvider(compiledResourceDirectory)) {
                    Registry = {
                        [1] = new("Blaster Texture", []),
                        [2] = new("Blaster Sprite", []),
                        [3] = new("Blaster Ray Texture", []),
                        [4] = new("Blaster Ray Sprite", []),
                    }
                },
            },
            Logger = _logger,
        };
    }

    public static ImportingOperation Import(ResourceID rid) => _importEnv.Import(rid);

    public static ReleaseStatus Release(ResourceID rid) => _importEnv.Release(rid);
    public static ReleaseStatus Release(ResourceHandle handle) => _importEnv.Release(handle);
}
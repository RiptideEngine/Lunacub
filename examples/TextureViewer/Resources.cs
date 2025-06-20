using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public static class Resources {
    private static bool _initialized;

    private static Renderer _renderer = null!;
    private static ILogger _logger = null!;
    
    private static FileResourceLibrary _resourceLibrary = null!;
    private static ImportEnvironment _importEnv = null!;
    
    public static void Initialize(Renderer renderer, ILogger logger) {
        if (Interlocked.Exchange(ref _initialized, true)) {
            throw new InvalidOperationException("Resources has already been initialized.");
        }

        _renderer = renderer;
        _logger = logger;

        BuildResources();
        CreateImportEnvironment();
    }
    
    private static void BuildResources() {
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource Outputs");
        string reportDirectory = Path.Combine(outputDirectory, "Reports");
        string resourcesDirectory = Path.Combine(outputDirectory, "Resources");

        Directory.CreateDirectory(reportDirectory);
        Directory.CreateDirectory(resourcesDirectory);
        
        using BuildEnvironment environment = new BuildEnvironment(new FileOutputSystem(reportDirectory, resourcesDirectory)) {
            Importers = {
                [nameof(Texture2DImporter)] = new Texture2DImporter(),
            },
            Processors = {
            },
            SerializerFactories = {
                new Texture2DSerializerFactory(),
            },
            Logger = _logger,
        };
        
        string resourceDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        string resourceRegistryPath = Path.Combine(resourceDirectoryPath, "ResourceRegistry.json");

        if (!Path.Exists(resourceRegistryPath)) {
            throw new("Missing Resource Registry.");
        }

        using (FileStream fs = new FileStream(resourceRegistryPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            var resourceElements = JsonSerializer.Deserialize<Dictionary<ResourceID, ResourceElement>>(fs)!;

            foreach ((var id, ResourceElement element) in resourceElements) {
                environment.Resources.Add(id, new(id.ToString(), [], new() {
                    Provider = new FileResourceProvider(Path.Combine(resourceDirectoryPath, element.Path)),
                    Options = new() {
                        ImporterName = element.ImporterName,
                        ProcessorName = element.ProcessorName,
                    },
                }));
            }
        }

        var result = environment.BuildResources();

        StringBuilder sb = new StringBuilder(1024);
        sb.AppendLine("Resource building result:");
        sb.Append("- Resource Count: ").Append(environment.Resources.Count).AppendLine(".");
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

        _resourceLibrary = new(resourcesDirectory);
    }

    private static void CreateImportEnvironment() {
        _importEnv = new() {
            Deserializers = {
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
                _resourceLibrary,
            },
            Logger = _logger,
        };
    }

    public static ImportingOperation<T> Import<T>(ResourceID rid) where T : class => _importEnv.Import<T>(rid);

    public static ReleaseStatus Release(ResourceID rid) => _importEnv.Release(rid);
    public static ReleaseStatus Release<T>(ResourceHandle<T> handle) where T : class => _importEnv.Release(handle);

    // public static IEnumerable<ResourceID> EnumerateResourceIds() => _importEnv.Libraries.SelectMany(x => x);

    private readonly record struct ResourceElement(
        string Path,
        [property: JsonPropertyName("Importer")] string ImporterName,
        [property: JsonPropertyName("Processor")] string? ProcessorName
    );
}
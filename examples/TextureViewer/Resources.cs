using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Core;
using Caxivitual.Lunacub.Importing;
using Caxivitual.Lunacub.Importing.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FileSourceProvider = Caxivitual.Lunacub.Importing.Core.FileSourceProvider;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

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

        BuildResourceLibrary library = new(new Building.Core.FileSourceProvider(resourceDirectoryPath));

        using (FileStream fs = new FileStream(resourceRegistryPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            var resourceElements = JsonSerializer.Deserialize<Dictionary<ResourceID, ResourceElement>>(fs)!;

            foreach ((var id, ResourceElement element) in resourceElements) {
                library.Registry.Add(id, new(element.Name, element.Tags, new() {
                    Addresses = new(Path.Combine(resourceDirectoryPath, element.Path)),
                    Options = new() {
                        ImporterName = element.ImporterName,
                        ProcessorName = element.ProcessorName,
                    },
                }));
            }
        }

        environment.Libraries.Add(library);

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
                [nameof(Texture2DDeserializer)] = new Texture2DDeserializer(_renderer),
            },
            Disposers = {
                Disposer.Create(obj => {
                    if (obj is not IDisposable disposable) return false;

                    disposable.Dispose();
                    return true;
                }),
            },
            Logger = _logger,
        };

        ImportResourceLibrary library = new(new FileSourceProvider(compiledResourceDirectory));

        using (FileStream registryStream = File.OpenRead(Path.Combine(compiledResourceDirectory, "__registry"))) {
            foreach ((var resourceId, var element) in JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element>>(registryStream)!) {
                library.Registry.Add(resourceId, element);
            }
        }

        _importEnv.Libraries.Add(library);
    }

    public static ImportingOperation Import(ResourceID rid) => _importEnv.Import(rid);

    public static ReleaseStatus Release(ResourceID rid) => _importEnv.Release(rid);
    public static ReleaseStatus Release<T>(ResourceHandle<T> handle) where T : class => _importEnv.Release(handle);

    // public static IEnumerable<ResourceID> EnumerateResourceIds() => _importEnv.Libraries.SelectMany(x => x);

    private readonly record struct ResourceElement(
        string Name,
        ImmutableArray<string> Tags,
        string Path,
        [property: JsonPropertyName("Importer")] string ImporterName,
        [property: JsonPropertyName("Processor")] string? ProcessorName
    );
}
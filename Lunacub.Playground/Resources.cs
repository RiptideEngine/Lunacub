using Caxivitual.Lunacub;
using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Importing;
using System.Diagnostics;
using System.Reflection;

namespace Lunacub.Playground;

public static class Resources {
    public static ImportEnvironment? ImportEnvironment { get; private set; }
    
    public static bool IsInitialized => ImportEnvironment != null;

    public static void Initialize(ShaderingSystem shaderingSystem) {
        if (ImportEnvironment != null) throw new InvalidOperationException();

        string reportDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
        string resourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        
        Directory.CreateDirectory(reportDirectory);
        Directory.CreateDirectory(resourceDirectory);

        BuildEnvironment buildEnv = new(new FileOutputSystem(reportDirectory, resourceDirectory));

        buildEnv.Importers.Add(nameof(Texture2DImporter), new Texture2DImporter());
        buildEnv.Serializers.Add(new Texture2DSerializer());
        
        buildEnv.Importers.Add(nameof(ShaderImporter), new ShaderImporter(shaderingSystem));
        buildEnv.Serializers.Add(new ShaderSerializer());
        
        buildEnv.Importers.Add(nameof(SamplerImporter), new SamplerImporter());
        buildEnv.Serializers.Add(new SamplerSerializer());
        
        buildEnv.Importers.Add(nameof(MeshSerializer), new MeshImporter());
        buildEnv.Serializers.Add(new MeshSerializer());
        
        string uncompiledResourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UncompiledResources");
        
        buildEnv.Resources.Add(ResourceID.Parse("7ae127d7df4853bc8e13f5b18cd893aa"), Path.Combine(uncompiledResourceDirectory, "Texture.png"), new() {
            ImporterName = nameof(Texture2DImporter),
        });
        
        buildEnv.Resources.Add(ResourceID.Parse("d07d31086d805186899523663761f74f"), Path.Combine(uncompiledResourceDirectory, "Shader.hlsl"), new() {
            ImporterName = nameof(ShaderImporter),
        });
        
        buildEnv.Resources.Add(ResourceID.Parse("0195d7cfdb687a7593979168e2e62a7c"), Path.Combine(uncompiledResourceDirectory, "Sampler.json"), new() {
            ImporterName = nameof(SamplerImporter),
        });
        buildEnv.Resources.Add(ResourceID.Parse("febcd85870715ddea807221fb5b71dc8"), Path.Combine(uncompiledResourceDirectory, "cube2.obj"), new() {
            ImporterName = nameof(MeshSerializer),
        });

        BuildingResult result = buildEnv.BuildResources();

        foreach ((var rid, var resourceResult) in result.ResourceResults) {
            if (resourceResult.IsSuccess) continue;
            
            Console.WriteLine($"Resource {rid} build failure: {resourceResult.Status}{(resourceResult.Exception == null ? string.Empty : $" ({resourceResult.Exception.SourceException})")}.");
        }
        
        ImportEnvironment = new() {
            Input = {
                Libraries = {
                    new FileResourceLibrary(Guid.Parse("9316e37d56f257878446949b465fa4d5"), resourceDirectory),
                },
            },
            Disposers = {
                Disposer.Create(obj => {
                    if (obj is not IDisposable disposable) return false;
                    
                    disposable.Dispose();
                    return true;
                }),
            },
        };
    }

    public static ResourceHandle<T> Import<T>(ResourceID rid) where T : class {
        if (ImportEnvironment == null) return default;
        
        return ImportEnvironment.Import<T>(rid);
    }

    public static ReleaseStatus Release<T>(ResourceHandle<T> handle) where T : class {
        if (ImportEnvironment == null) return ReleaseStatus.Unspecified;
        
        return ImportEnvironment.Release(handle);
    }

    public static void Shutdown() {
        ImportEnvironment?.Dispose();
    }
}
using System.Collections.Immutable;
using System.Reflection;
using BuildMemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;
using ImportMemorySourceProvider = Caxivitual.Lunacub.Importing.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class PrebuildResourcesFixture {
    public static readonly ResourceID SimpleResourceStart = 1;
    public const int SimpleResourceCount = 3;

    public static readonly ResourceID ReferencingResourceNoReference = 101;
    public static readonly ResourceID ReferencingResource2ObjectsChainA = 102;
    public static readonly ResourceID ReferencingResource2ObjectsChainB = 103;
    public static readonly ResourceID ReferencingResource4ObjectsChainA = 104;
    public static readonly ResourceID ReferencingResource4ObjectsChainB = 105;
    public static readonly ResourceID ReferencingResource4ObjectsChainC = 106;
    public static readonly ResourceID ReferencingResource4ObjectsChainD = 107;
    public static readonly ResourceID ReferencingResourceSelfReference = 108;
    public static readonly ResourceID ReferencingResourceCircularReferenceA = 109;
    public static readonly ResourceID ReferencingResourceCircularReferenceB = 110;
    public static readonly ResourceID ReferencingResourceReferenceUnregistered = 111;

    public static readonly ResourceID ConfigurableResourceBinary = 201;
    public static readonly ResourceID ConfigurableResourceJson = 202;

    public static readonly ResourceID DeferrableResource = 301;

    private readonly Caxivitual.Lunacub.Building.Core.MemoryOutputSystem _buildOutput;
    private readonly IReadOnlyDictionary<Type, ImmutableArray<Type>> _componentTypes;
    
    public PrebuildResourcesFixture() {
        List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetReferencedAssemblies().Contains(Assembly.GetExecutingAssembly().GetName()))
            .Append(Assembly.GetExecutingAssembly()).SelectMany(x => x.ExportedTypes)
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .ToList();
        
        _componentTypes = new Dictionary<Type, ImmutableArray<Type>> {
            [typeof(Importer)] = [..types.Where(x => x.IsSubclassOf(typeof(Importer)))],
            [typeof(Processor)] = [..types.Where(x => x.IsSubclassOf(typeof(Processor)))],
            [typeof(SerializerFactory)] = [..types.Where(x => x.IsSubclassOf(typeof(SerializerFactory)))],
            [typeof(Deserializer)] = [..types.Where(x => x.IsSubclassOf(typeof(Deserializer)))],
        };
        
        var buildSourceProvider = new BuildMemorySourceProvider();
        var buildLibrary = new BuildResourceLibrary(buildSourceProvider);

        AppendBuildingResources(buildSourceProvider, buildLibrary);

        _buildOutput = new();
        
        using BuildEnvironment buildEnv = new BuildEnvironment(_buildOutput) {
            Libraries = {
                buildLibrary,
            },
        };
        
        foreach (var type in _componentTypes[typeof(Importer)]) {
            buildEnv.Importers[type.Name] = (Importer)Activator.CreateInstance(type)!;
        }
        
        foreach (var type in _componentTypes[typeof(Processor)]) {
            buildEnv.Processors[type.Name] = (Processor)Activator.CreateInstance(type)!;
        }
        
        foreach (var type in _componentTypes[typeof(SerializerFactory)]) {
            buildEnv.SerializerFactories.Add((SerializerFactory)Activator.CreateInstance(type)!);
        }

        buildEnv.BuildResources();
        
        // Create Import Environment
        var importSourceProvider = new ImportMemorySourceProvider();
        var importLibrary = new ImportResourceLibrary(importSourceProvider);

        foreach ((var resourceId, var registryElement) in _buildOutput.OutputRegistry) {
            importSourceProvider.Resources.Add(resourceId, _buildOutput.OutputResources[resourceId].Item1);
            importLibrary.Registry.Add(resourceId, registryElement);
        }
    }

    private static void AppendBuildingResources(BuildMemorySourceProvider sourceProvider, BuildResourceLibrary library) {
        AppendSimpleResources(sourceProvider, library);
        AppendReferencingResources(sourceProvider, library);
        AppendConfigurableResources(sourceProvider, library);
        AppendDeferrableResource(sourceProvider, library);
        return;

        static void AppendSimpleResources(BuildMemorySourceProvider sourceProvider, BuildResourceLibrary library) {
            for (UInt128 i = SimpleResourceStart; i < (UInt128)SimpleResourceStart + (uint)SimpleResourceCount; i++) {
                string name = $"{nameof(SimpleResource)}{i}";
            
                sourceProvider.Sources
                    .Add(name, BuildMemorySourceProvider.AsUtf8($$"""{"Value":{{i - SimpleResourceStart + 1}}}""", DateTime.MinValue));

                library.Registry.Add(i, new(name, [], new() {
                    Addresses = new(name),
                    Options = new(nameof(SimpleResourceImporter)),
                }));
            }
        }

        static void AppendReferencingResources(BuildMemorySourceProvider sourceProvider, BuildResourceLibrary library) {
            // No reference.
            AppendReferencingResource(ReferencingResourceNoReference, $"{nameof(ReferencingResource)}_NoReference", 1, ResourceID.Null);
            
            // 2 objects chain.
            AppendReferencingResource(ReferencingResource2ObjectsChainA, $"{nameof(ReferencingResource)}_2ObjectsChainA", 1, ReferencingResource2ObjectsChainB);
            AppendReferencingResource(ReferencingResource2ObjectsChainB, $"{nameof(ReferencingResource)}_2ObjectsChainB", 2, ResourceID.Null);
            
            // 4 objects chain
            AppendReferencingResource(ReferencingResource4ObjectsChainA, $"{nameof(ReferencingResource)}_4ObjectsChainA", 1, ReferencingResource4ObjectsChainB);
            AppendReferencingResource(ReferencingResource4ObjectsChainB, $"{nameof(ReferencingResource)}_4ObjectsChainB", 2, ReferencingResource4ObjectsChainC);
            AppendReferencingResource(ReferencingResource4ObjectsChainC, $"{nameof(ReferencingResource)}_4ObjectsChainC", 3, ReferencingResource4ObjectsChainD);
            AppendReferencingResource(ReferencingResource4ObjectsChainD, $"{nameof(ReferencingResource)}_4ObjectsChainD", 4, ResourceID.Null);
            
            // Self reference
            AppendReferencingResource(ReferencingResourceSelfReference, $"{nameof(ReferencingResource)}_SelfReference", 1, ReferencingResourceSelfReference);
            
            // Circular reference
            AppendReferencingResource(ReferencingResourceCircularReferenceA, $"{nameof(ReferencingResource)}_CircularReferenceA", 1, ReferencingResourceCircularReferenceB);
            AppendReferencingResource(ReferencingResourceCircularReferenceB, $"{nameof(ReferencingResource)}_CircularReferenceB", 2, ReferencingResourceCircularReferenceA);
            
            // Reference unregistered.
            AppendReferencingResource(ReferencingResourceReferenceUnregistered, $"{nameof(ReferencingResource)}_ReferenceUnregistered", 1, ReferencingResourceReferenceUnregistered.Value + 1);
            return;

            void AppendReferencingResource(ResourceID id, string name, int value, ResourceID reference) {
                sourceProvider.Sources
                    .Add(name, BuildMemorySourceProvider.AsUtf8($$"""{"Value":{{value}},"Reference":{{reference}}}""", DateTime.MinValue));
            
                library.Registry.Add(id, new(name, [], new() {
                    Addresses = new(name),
                    Options = new(nameof(ReferencingResourceImporter)),
                }));
            }
        }

        static void AppendConfigurableResources(BuildMemorySourceProvider sourceProvider, BuildResourceLibrary library) {
            BuildMemorySourceProvider.Element element = BuildMemorySourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue);
            
            sourceProvider.Sources
                .Add($"{nameof(ConfigurableResource)}_Binary", element);
            
            library.Registry.Add(ConfigurableResourceBinary, new($"{nameof(ConfigurableResource)}_Binary", [], new() {
                Addresses = new($"{nameof(ConfigurableResource)}_Binary"),
                Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Binary)),
            }));
            
            sourceProvider.Sources
                .Add($"{nameof(ConfigurableResource)}_Json", element);
            
            library.Registry.Add(ConfigurableResourceJson, new($"{nameof(ConfigurableResource)}_Json", [], new() {
                Addresses = new($"{nameof(ConfigurableResource)}_Json"),
                Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Json)),
            }));
        }

        static void AppendDeferrableResource(BuildMemorySourceProvider sourceProvider, BuildResourceLibrary library) {
            sourceProvider.Sources
                .Add("DeferrableResource", BuildMemorySourceProvider.AsUtf8(string.Empty, DateTime.MinValue));

            library.Registry.Add(DeferrableResource, new("DeferrableResource", [], new() {
                Addresses = new("DeferrableResource"),
                Options = new(nameof(DeferrableResourceImporter)),
            }));
        }
    }

    public ImportEnvironment CreateImportEnvironment() {
        var importSourceProvider = new ImportMemorySourceProvider();
        var importLibrary = new ImportResourceLibrary(importSourceProvider);

        foreach ((var resourceId, var registryElement) in _buildOutput.OutputRegistry) {
            importSourceProvider.Resources.Add(resourceId, _buildOutput.OutputResources[resourceId].Item1);
            importLibrary.Registry.Add(resourceId, registryElement);
        }
        
        ImportEnvironment environment = new() {
            Libraries = {
                importLibrary,
            },
        };
        
        foreach (var type in _componentTypes[typeof(Deserializer)]) {
            environment.Deserializers[type.Name] = (Deserializer)Activator.CreateInstance(type)!;
        }

        return environment;
    }
}
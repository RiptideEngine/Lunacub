// using Caxivitual.Lunacub.Building.Core;
//
// namespace Caxivitual.Lunacub.Tests.Importing;
//
// public partial class ImportEnvironmentTests : IClassFixture<ComponentsFixture>, IDisposable {
//     private readonly ComponentsFixture _componentsFixture;
//
//     private readonly MockFileSystem _fileSystem;
//     private readonly BuildEnvironment _buildEnv;
//     private readonly ImportEnvironment _importEnv;
//     private readonly ITestOutputHelper _output;
//     
//     public ImportEnvironmentTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
//         _componentsFixture = componentsFixture;
//         _output = output;
//         DebugHelpers.RedirectConsoleOutput(output);
//
//         _fileSystem = new();
//         
//         _buildEnv = new(new FileResourceProvider(), new MockOutputSystem(_fileSystem));
//         _importEnv = new();
//
//         foreach (var type in _componentsFixture.ComponentTypes[typeof(Importer)]) {
//             _buildEnv.Importers.Add(type.Name, (Importer)Activator.CreateInstance(type)!);
//         }
//         
//         foreach (var type in _componentsFixture.ComponentTypes[typeof(Processor)]) {
//             _buildEnv.Processors.Add(type.Name, (Processor)Activator.CreateInstance(type)!);
//         }
//         
//         foreach (var type in _componentsFixture.ComponentTypes[typeof(SerializerFactory)]) {
//             _buildEnv.SerializerFactories.Add((SerializerFactory)Activator.CreateInstance(type)!);
//         }
//         
//         foreach (var type in _componentsFixture.ComponentTypes[typeof(Deserializer)]) {
//             _importEnv.Deserializers.Add(type.Name, (Deserializer)Activator.CreateInstance(type)!);
//         }
//     }
//     
//     public void Dispose() {
//         _importEnv.Dispose();
//         _buildEnv.Dispose();
//         
//         GC.SuppressFinalize(this);
//     }
//
//     private BuildingResult BuildResources(params ResourceID[] rids) {
//         foreach (var rid in rids) {
//             _componentsFixture.RegisterResourceToBuild(_buildEnv, rid);
//         }
//         
//         return _buildEnv.BuildResources();
//     }
// }
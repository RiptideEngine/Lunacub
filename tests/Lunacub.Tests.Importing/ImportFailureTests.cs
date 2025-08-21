// namespace Caxivitual.Lunacub.Tests.Importing;
//
// public class ImportFailureTests : IClassFixture<ComponentsFixture>, IDisposable {
//     private readonly ImportEnvironment _importEnvironment;
//     
//     public ImportFailureTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
//         var buildSourceProvider = new BuildMemorySourceProvider();
//         buildSourceProvider.Sources.Add(nameof(SimpleResource), new([.."{\"Value\":1}"u8], DateTime.MinValue));
//         
//         MemoryOutputSystem _buildOutput = new();
//
//         using BuildEnvironment buildEnv = new BuildEnvironment(_buildOutput, new())
//             .AddLibrary(
//                 new BuildResourceLibrary(1, buildSourceProvider).AddRegistryElement(1, new(string.Empty, [], new() {
//                     Addresses = new(nameof(SimpleResource)),
//                     Options = new(nameof(SimpleResourceImporter)),
//                 }))
//             );
//         
//         var importSourceProvider = new ImportMemorySourceProvider();
//         var importLibrary = new ImportResourceLibrary(1, importSourceProvider);
//         
//         foreach ((var resourceId, var compiledBinary) in _buildOutput.Outputs[1].CompiledResources) {
//             importSourceProvider.Resources.Add(resourceId, compiledBinary.Item1);
//         }
//         
//         foreach ((var resourceId, var registryElement) in _buildOutput.Outputs[1].Registry) {
//             importLibrary.AddRegistryElement(resourceId, registryElement);
//         }
//         
//         _importEnvironment = new ImportEnvironment()
//             .SetLogger(output.BuildLogger())
//             .AddLibrary(importLibrary);
//         
//         componentsFixture.ApplyComponents(_importEnvironment);
//     }
//     
//     public void Dispose() {
//         _importEnvironment.Dispose();
//         
//         GC.SuppressFinalize(this);
//     }
//     
//     [Fact]
//     public async Task FailureImport_UnregisteredResource_ReturnsCorrectStates() {
//         _importEnvironment.Libraries.Add(new(2, new Lunacub.Importing.Core.MemorySourceProvider()));
//
//         var operation = _importEnvironment.Import(new ResourceAddress(1, uint.MaxValue));
//         await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<ArgumentException>().WithMessage("*unregistered*");
//
//         operation.Status.Should().Be(ImportingStatus.Failed);
//         operation.UnderlyingContainer.CancellationTokenSource.Should().BeNull();
//         operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
//     }
//     
//     [Fact]
//     public async Task FailureImport_NullResourceStream_ReturnsCorrectStates() {
//         _importEnvironment.Libraries.Add(new(2, new NullStreamSourceProvider()) {
//             Registry = {
//                 [UInt128.MaxValue - 1] = new("Resource", []),
//             },
//         });
//
//         var operation = _importEnvironment.Import(new ResourceAddress(2, UInt128.MaxValue - 1));
//         await new Func<Task<ResourceHandle>>(() => operation.Task).Should().ThrowAsync<InvalidOperationException>().WithMessage("*null*stream*");
//         
//         operation.Status.Should().Be(ImportingStatus.Failed);
//         operation.UnderlyingContainer.CancellationTokenSource.Should().BeNull();
//         operation.UnderlyingContainer.ReferenceCount.Should().Be(0);
//     }
//
//     [Fact]
//     public void FailureDeserialization_ExceptionThrown_ReturnsCorrectState() {
//         // TODO
//     }
// }
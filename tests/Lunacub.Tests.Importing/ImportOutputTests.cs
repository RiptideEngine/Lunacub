// namespace Caxivitual.Lunacub.Tests.Importing;
//
// [Collection<PrebuildResourcesCollectionFixture>]
// public class ImportOutputTests : IDisposable {
//     private readonly ImportEnvironment _importEnvironment;
//     
//     public ImportOutputTests(PrebuildResourcesFixture fixture, ITestOutputHelper output) {
//         _importEnvironment = fixture.CreateImportEnvironment();
//         _importEnvironment.Logger = output.BuildLogger();
//     }
//     
//     public void Dispose() {
//         _importEnvironment.Dispose();
//         
//         GC.SuppressFinalize(this);
//     }
//     
//     [Fact]
//     public async Task ImportSimpleResource_ReturnsCorrectObject() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.SimpleResourceStart)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//     
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.SimpleResourceStart));
//         handle.Value.Should().BeOfType<SimpleResource>().Which.Value.Should().Be(1);
//     }
//     
//     [Fact]
//     public async Task ImportConfigurableResource_WithBinaryOption_ReturnsCorrectObject() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ConfigurableResourceBinary)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//     
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ConfigurableResourceBinary));
//         handle.Value.Should().BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
//     }
//     
//     [Fact]
//     public async Task ImportConfigurableResource_WithJsonOption_ReturnsCorrectObject() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ConfigurableResourceJson)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//     
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ConfigurableResourceJson));
//         handle.Value.Should().BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
//     }
//     
//     [Fact]
//     public async Task ImportReferencingResource_UnregisteredReference_ReturnsCorrectObjects() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResourceReferenceUnregistered)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//         
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResourceReferenceUnregistered));
//
//         var resource = handle.Value.Should().BeOfType<ReferencingResource>().Which;
//         resource.Value.Should().Be(1);
//         resource.Reference.Should().BeNull();
//     }
//     
//     [Fact]
//     public async Task ImportReferencingResource_2ObjectsChain_ReturnsCorrectObjects() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResource2ObjectsChainA)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//         
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResource2ObjectsChainA));
//         var resource1 = handle.Value.Should().BeOfType<ReferencingResource>().Which;
//         resource1.Value.Should().Be(1);
//     
//         var resource2 = resource1.Reference;
//         resource2.Should().NotBeNull();
//         resource2!.Value.Should().Be(2);
//         resource2.Reference.Should().BeNull();
//     }
//     
//     [Fact]
//     public async Task ImportReferencingResource_4ObjectsChain_ReturnsCorrectObjects() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResource4ObjectsChainA)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//         
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResource4ObjectsChainA));
//         var resource1 = handle.Value.Should().BeOfType<ReferencingResource>().Which;
//         resource1.Value.Should().Be(1);
//         
//         var resource2 = resource1.Reference;
//         resource2.Should().NotBeNull();
//         resource2!.Value.Should().Be(2);
//         resource2.Reference.Should().NotBeNull();
//         
//         var resource3 = resource2.Reference;
//         resource3.Should().NotBeNull();
//         resource3!.Value.Should().Be(3);
//         resource3.Reference.Should().NotBeNull();
//         
//         var resource4 = resource3.Reference;
//         resource4.Should().NotBeNull();
//         resource4!.Value.Should().Be(4);
//         resource4.Reference.Should().BeNull();
//     }
//
//     [Fact]
//     public async Task ImportReferencingResource_MismatchReferenceType_ReleasesMismatchResource() {
//         var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResourceMismatchReferenceType)).Task)
//             .Should()
//             .NotThrowAsync())
//             .Which;
//         
//         handle.Address.Should().Be(new ResourceAddress(1, PrebuildResourcesFixture.ReferencingResourceMismatchReferenceType));
//         var resource1 = handle.Value.Should().BeOfType<ReferencingResource>().Which;
//         resource1.Value.Should().Be(1);
//         resource1.Reference.Should().BeNull();
//         
//         _importEnvironment.GetResourceContainer(new ResourceAddress(1, PrebuildResourcesFixture.SimpleResourceStart)).Should().BeNull();
//     }
// }
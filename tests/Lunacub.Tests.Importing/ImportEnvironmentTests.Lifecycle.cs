// namespace Caxivitual.Lunacub.Tests.Importing;
//
// partial class ImportEnvironmentTests {
//     [Fact]
//     public void Release_WithDisposer_ShouldReleaseAndDisposeSuccessfully() {
//         ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
//         
//         BuildResources(rid);
//
//         _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
//         _importEnv.Disposers.Add(Disposer.Create(obj => {
//             if (obj is not IDisposable disposable) return false;
//
//             disposable.Dispose();
//             return true;
//         }));
//         
//         var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
//         fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
//
//         var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
//         resource.Disposed.Should().BeFalse();
//
//         new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
//         resource.Disposed.Should().BeTrue();
//     }
//     
//     [Fact]
//     public void Release_WithoutDisposer_ShouldReleaseButNotDispose() {
//         ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
//         
//         BuildResources(rid);
//
//         _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
//         
//         var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
//         fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
//
//         var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
//         resource.Disposed.Should().BeFalse();
//
//         new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.NotDisposed);
//         resource.Disposed.Should().BeFalse();
//     }
//
//     [Fact]
//     public void ReleaseReferenceCount_ShouldDisposeCorrectly() {
//         ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
//         
//         BuildResources(rid);
//
//         _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
//         _importEnv.Disposers.Add(Disposer.Create(obj => {
//             if (obj is not IDisposable disposable) return false;
//
//             disposable.Dispose();
//             return true;
//         }));
//         
//         var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
//         fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
//
//         var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
//         resource.Disposed.Should().BeFalse();
//         
//         var reference = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
//         reference.Disposed.Should().BeFalse();
//
//         resource.Should().BeSameAs(reference);
//
//         new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
//         resource.Disposed.Should().BeFalse();
//         
//         new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
//         resource.Disposed.Should().BeTrue();
//     }
//
//     [Fact]
//     public void Release_WithReference_AlsoReleaseReferences() {
//         ResourceID rid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
//         ResourceID rid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
//         
//         BuildResources(rid1, rid2);
//         
//         _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
//         _importEnv.Disposers.Add(Disposer.Create(obj => {
//             if (obj is not IDisposable disposable) return false;
//
//             disposable.Dispose();
//             return true;
//         }));
//         
//         var dependent = new Func<ResourceHandle<ReferenceResource>>(() => _importEnv.Import<ReferenceResource>(rid1)).Should().NotThrow().Which;
//         var dependency = new Func<ResourceHandle<ReferenceResource>>(() => _importEnv.Import<ReferenceResource>(rid2)).Should().NotThrow().Which;
//
//         dependent.Value!.Disposed.Should().BeFalse();
//         dependency.Value!.Disposed.Should().BeFalse();
//         
//         _importEnv.Release(dependent).Should().Be(ReleaseStatus.Success);
//         
//         dependent.Value!.Disposed.Should().BeTrue();
//         dependency.Value!.Disposed.Should().BeTrue();
//     }
// }